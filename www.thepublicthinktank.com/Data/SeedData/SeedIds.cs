﻿using System;

namespace atlas_the_public_think_tank.Data.SeedData
{
    /// <summary>
    /// Centralized storage for all seed data IDs to enable consistent references across seed data classes
    /// Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).
    /// </summary>
    public static class SeedIds
    {
        // User IDs
        public static class Users
        {
            public static readonly Guid CooperBarker = new Guid("1a61454c-5b83-4aab-8661-96d6dffbee30");
            public static readonly Guid AmeliaKnight = new Guid("1a61454c-5b83-4aab-8661-96d6dffbee31");
        }

        // Category IDs
        public static class Categories
        {
            public static readonly Guid GlobalCooperation = new Guid("81f910e0-39a4-4b44-88ca-fd3c30af4a25");
            public static readonly Guid SustainableDevelopment = new Guid("25487e1f-b167-4666-a20c-dec2e4b5f413");
            public static readonly Guid EquitableAccess = new Guid("a8fb4691-8c1f-4e7d-b315-b042097e6395");
            public static readonly Guid InnovationAndTechnology = new Guid("f5c35e6a-8c4f-4556-b6c1-4448b26d1bcb");
            public static readonly Guid EffectiveGovernance = new Guid("26c867f2-48c6-4bd5-b36a-9f7325431ad3");
            public static readonly Guid EducationAndAwareness = new Guid("d2c7a605-a621-4b14-8d51-e2df0cecae1a");
            public static readonly Guid CulturalUnderstanding = new Guid("0950f1d0-5c03-4f3a-9015-c4bb3c0e7620");
            public static readonly Guid ResilienceAndAdaptability = new Guid("3ce7d7d2-176d-4b72-8d98-4b97b49ed0c1");
        }

        // Add more entity ID sections as needed (Posts, Comments, etc.)
        public static class Posts
        {
            // Example post IDs
            public static readonly Guid Post1 = new Guid("7e2f3a6c-718f-4c07-b1e9-71fe849344d1");
            public static readonly Guid Post2 = new Guid("c1f7d293-4f5c-4b2e-a65b-3ef245755c01");
        }        
        public static class IssueCategories
        {
            public static readonly Guid ClimateChangeGlobalCoop = new Guid("7e2d3b6a-717f-4c07-b1e9-71fe849344d8");
            public static readonly Guid ClimateSustainable = new Guid("6f5e4d3c-2b1a-49f8-87e5-12d3c4b5a678");
            // etc.
        }        
        public static class Scopes
        {
            public static readonly Guid Global = new Guid("a1e7c6b3-d5e2-4f8a-9c3b-8d7e6f5d4c2b");
            public static readonly Guid National = new Guid("b2d8f1a7-e4c9-4f8a-8d5f-7e6c9d8b3a2f");
            public static readonly Guid Local = new Guid("c3b9a2d8-f1e7-4f8a-9c3b-8d7e6f5d4c2b");
            public static readonly Guid Individual = new Guid("d4c9b3a2-f8e7-4f8a-9c3b-8d7e6f5d4c2b");
        }

                  
        public static class Issues
        {
            public static readonly Guid ClimateChangeSolutions = new Guid("fd43657c-a0a8-4721-a6b5-3f23e35088fc");
            public static readonly Guid UrbanPlanningInnovations = new Guid("4aebb16c-b474-4c14-9e5e-4548134cadc8");
            public static readonly Guid RenewableEnergyTransition = new Guid("c246d67c-427c-40f9-8bb2-b0834e473f7b");
            public static readonly Guid EndangeredSpeciesDecline = new Guid("b3a72e5d-7c18-4e9f-8d24-67a2c6f35b1d");
            public static readonly Guid OrcaPopulationDecline = new Guid("e5d8f6a9-3b7c-42e1-9d85-7f63a4b5c28d");
            // Add more issue IDs as needed
        }
        
     
    }
}