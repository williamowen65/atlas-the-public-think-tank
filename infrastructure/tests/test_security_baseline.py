"""Blocking, offline policy checks for Atlas's Terraform security baseline.

These tests parse HCL, not Terraform state or Azure. They protect a few
deliberate project decisions; Trivy supplies the broader advisory analysis.
Run from any directory: python -m unittest discover -s infrastructure/tests
Requires python-hcl2==8.1.4 (installed by the IaC GitHub Action).
"""
from __future__ import annotations

import unittest
from pathlib import Path

import hcl2


ROOT = Path(__file__).resolve().parents[1] / "terraform"


def read_hcl(relative_path: str) -> dict:
    with (ROOT / relative_path).open(encoding="utf-8") as source:
        return hcl2.load(source)


def resource(relative_path: str, resource_type: str, name: str) -> dict:
    for block in read_hcl(relative_path).get("resource", []):
        if resource_type in block and name in block[resource_type]:
            return block[resource_type][name]
    raise AssertionError(f"Expected Terraform resource: {resource_type}.{name}")


def variable(name: str) -> dict:
    for block in read_hcl("variables.tf")["variable"]:
        if name in block:
            return block[name]
    raise AssertionError(f"Expected Terraform variable: {name}")


class TerraformSecurityBaselineTests(unittest.TestCase):
    def test_web_apps_require_https_tls_and_disable_ftp(self) -> None:
        web = resource("modules/webapp/main.tf", "azurerm_linux_web_app", "atlas")
        self.assertIs(web["https_only"], True)
        self.assertIs(web["ftp_publish_basic_authentication_enabled"], False)
        site = web["site_config"][0]
        self.assertEqual(site["ftps_state"], "Disabled")
        self.assertGreaterEqual(float(site["minimum_tls_version"]), 1.2)

    def test_web_apps_have_managed_identity(self) -> None:
        web = resource("modules/webapp/main.tf", "azurerm_linux_web_app", "atlas")
        self.assertEqual(web["identity"][0]["type"], "SystemAssigned")

    def test_sql_network_access_is_allowlist_only(self) -> None:
        sql_path = "modules/sql/main.tf"
        server = resource(sql_path, "azurerm_mssql_server", "atlas")
        self.assertGreaterEqual(float(server["minimum_tls_version"]), 1.2)
        # SQL public endpoint is intentional while the F1 plan has no private-link
        # integration. Reachability must be denied until *specific* IPs are added.
        self.assertIs(server["public_network_access_enabled"], True)
        rule = resource(sql_path, "azurerm_mssql_firewall_rule", "approved")
        self.assertIn("var.allowed_ipv4", rule["for_each"])
        self.assertIn("each.value", rule["start_ip_address"])
        self.assertIn("each.value", rule["end_ip_address"])
        allowed = variable("sql_allowed_ipv4")
        self.assertEqual(allowed["default"], {})
        condition = allowed["validation"][0]["condition"]
        self.assertIn('ip != "0.0.0.0"', condition)

    def test_sql_free_offer_is_explicit_and_databases_protected(self) -> None:
        database = resource("modules/sql/main.tf", "azapi_resource", "variant")
        self.assertIs(database["body"]["properties"]["useFreeLimit"], True)
        self.assertEqual(database["body"]["properties"]["freeLimitExhaustionBehavior"], "AutoPause")
        self.assertEqual(database["body"]["sku"]["tier"], "GeneralPurpose")
        self.assertIs(database["lifecycle"][0]["prevent_destroy"], True)

    def test_free_student_defaults_and_isolated_demo(self) -> None:
        self.assertEqual(variable("app_service_sku")["default"], "F1")
        self.assertIs(variable("enable_demo")["default"], True)

    def test_sql_admin_password_not_hardcoded(self) -> None:
        server = resource("modules/sql/main.tf", "azurerm_mssql_server", "atlas")
        self.assertIn("var.admin_password", server["administrator_login_password"])
        self.assertIs(variable("sql_admin_password")["sensitive"], True)


if __name__ == "__main__":
    unittest.main()
