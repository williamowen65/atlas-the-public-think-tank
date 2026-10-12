# AI instructions for Atlas.Persistence

When changing EF Core mappings or schema:

- Do not create, generate, or hand-edit migration files (`Migrations/*.cs`, including designer files).
- Do not hand-edit the EF model snapshot. Leave it at the last developer-generated migration so EF can detect the pending model change.
- Make the model/configuration changes and relevant tests and documentation.
- Include the exact EF CLI commands in the PR summary for the developer to run from the solution root in their terminal. Use `--project src/Atlas.Persistence` and a descriptive migration name. Include tool restore, migration creation, pending-model verification, database update, and relevant tests.
- Clearly state that the developer must generate, review, and commit the migration, designer, and snapshot changes before migration-dependent tests/CI can pass. Do not report migration-dependent behavior as verified before those steps are completed.
- Do not apply migrations to a database on the developer's behalf.

Existing developer-generated migrations remain version controlled. If a user explicitly asks to remove an AI-added migration, remove that migration and its designer and restore the prior snapshot without modifying unrelated migration history.
