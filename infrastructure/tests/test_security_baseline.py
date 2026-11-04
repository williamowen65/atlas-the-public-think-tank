"""Blocking, offline policy checks for Atlas's Terraform security baseline.

SQL public access is forbidden. App Service must use VNet integration and SQL
must be reached by a Private Endpoint with a linked private DNS zone.

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

    def test_sql_has_no_public_endpoint_or_public_firewall_rules(self) -> None:
        sql_path = "modules/sql/main.tf"
        server = resource(sql_path, "azurerm_mssql_server", "atlas")
        self.assertGreaterEqual(float(server["minimum_tls_version"]), 1.2)
        self.assertIs(server["public_network_access_enabled"], False)
        resources = read_hcl(sql_path).get("resource", [])
        self.assertFalse(any("azurerm_mssql_firewall_rule" in block for block in resources))

    def test_sql_is_reached_only_through_private_link_and_dns(self) -> None:
        path = "modules/network/main.tf"
        pe = resource(path, "azurerm_private_endpoint", "sql")
        self.assertIn("azurerm_subnet.private_endpoints.id", pe["subnet_id"])
        self.assertIn("var.sql_server_id", pe["private_service_connection"][0]["private_connection_resource_id"])
        self.assertIn("sqlServer", pe["private_service_connection"][0]["subresource_names"])
        private_dns = resource(path, "azurerm_private_dns_zone", "sql")
        self.assertEqual(private_dns["name"], "privatelink.database.windows.net")
        link = resource(path, "azurerm_private_dns_zone_virtual_network_link", "sql")
        self.assertIn("azurerm_virtual_network.atlas.id", link["virtual_network_id"])

    def test_network_subnets_and_sql_firewall_are_restrictive(self) -> None:
        path = "modules/network/main.tf"
        app = resource(path, "azurerm_subnet", "app")
        self.assertIn("Microsoft.Web/serverFarms", app["delegation"][0]["service_delegation"][0]["name"])
        endpoint = resource(path, "azurerm_subnet", "private_endpoints")
        self.assertEqual(endpoint["private_endpoint_network_policies"], "NetworkSecurityGroupEnabled")
        security = resource(path, "azurerm_network_security_group", "sql")
        rules = {rule["name"]: rule for rule in security["security_rule"]}
        allow = rules["AllowAppToSql"]
        self.assertEqual(allow["access"], "Allow")
        self.assertEqual(allow["source_address_prefix"], "10.42.1.0/26")
        self.assertEqual(allow["destination_port_range"], "1433")
        deny = rules["DenyOtherVNetToSql"]
        self.assertEqual(deny["access"], "Deny")
        self.assertEqual(deny["source_address_prefix"], "VirtualNetwork")
        association = resource(path, "azurerm_subnet_network_security_group_association", "sql")
        self.assertIn("azurerm_subnet.private_endpoints.id", association["subnet_id"])

    def test_webapps_are_integrated_into_private_app_subnet(self) -> None:
        web = resource("modules/webapp/main.tf", "azurerm_linux_web_app", "atlas")
        self.assertIn("var.app_subnet_id", web["virtual_network_subnet_id"])
        self.assertIn("B1", variable("app_service_sku")["default"])

    def test_sql_free_offer_is_explicit_and_databases_protected(self) -> None:
        database = resource("modules/sql/main.tf", "azapi_resource", "variant")
        self.assertIs(database["body"]["properties"]["useFreeLimit"], True)
        self.assertEqual(database["body"]["properties"]["freeLimitExhaustionBehavior"], "AutoPause")
        self.assertEqual(database["body"]["sku"]["tier"], "GeneralPurpose")
        self.assertIs(database["lifecycle"][0]["prevent_destroy"], True)

    def test_free_student_defaults_and_isolated_demo(self) -> None:
        self.assertEqual(variable("app_service_sku")["default"], "B1")
        self.assertIs(variable("enable_demo")["default"], True)

    def test_sql_admin_password_not_hardcoded(self) -> None:
        server = resource("modules/sql/main.tf", "azurerm_mssql_server", "atlas")
        self.assertIn("var.admin_password", server["administrator_login_password"])
        self.assertIs(variable("sql_admin_password")["sensitive"], True)


if __name__ == "__main__":
    unittest.main()
