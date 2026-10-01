using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReferentialIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VoteRows_ParticipantId",
                table: "VoteRows",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionDefinitionRows_CreatedByParticipantId",
                table: "ReactionDefinitionRows",
                column: "CreatedByParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionAuditEntries_ActorParticipantId",
                table: "ReactionAuditEntries",
                column: "ActorParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ReactionAuditEntries_RelatedNodeReactionId",
                table: "ReactionAuditEntries",
                column: "RelatedNodeReactionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRows_ActorParticipantId",
                table: "NotificationRows",
                column: "ActorParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRows_RecipientParticipantId",
                table: "NotificationRows",
                column: "RecipientParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRows_AuthorId",
                table: "NodeRows",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRows_DescriptionId",
                table: "NodeRows",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRows_TypeId",
                table: "NodeRows",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeRequestedTypes_TypeId",
                table: "NodeRequestedTypes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_AppliedByParticipantId",
                table: "NodeReactionRows",
                column: "AppliedByParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_NodeId",
                table: "NodeReactionRows",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeReactionRows_ReactionDefinitionId",
                table: "NodeReactionRows",
                column: "ReactionDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_NodeParents_ParentNodeId",
                table: "NodeParents",
                column: "ParentNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_NodeId",
                table: "ModerationCaseRows",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_ReporterId",
                table: "ModerationCaseRows",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_RestoredBy",
                table: "ModerationCaseRows",
                column: "RestoredBy");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCaseRows_ReviewerId",
                table: "ModerationCaseRows",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentBlocks_BlockId",
                table: "DocumentBlocks",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityRows_OwnerParticipantId",
                table: "CommunityRows",
                column: "OwnerParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityNodeRows_AssociatedByParticipantId",
                table: "CommunityNodeRows",
                column: "AssociatedByParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityNodeRows_NodeId",
                table: "CommunityNodeRows",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityMembershipRows_ParticipantId",
                table: "CommunityMembershipRows",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_AuthorParticipantId",
                table: "CommentRows",
                column: "AuthorParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentRows_ParentCommentId",
                table: "CommentRows",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentRows_CommentRows_ParentCommentId",
                table: "CommentRows",
                column: "ParentCommentId",
                principalTable: "CommentRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentRows_ParticipantRows_AuthorParticipantId",
                table: "CommentRows",
                column: "AuthorParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityMembershipRows_CommunityRows_CommunityId",
                table: "CommunityMembershipRows",
                column: "CommunityId",
                principalTable: "CommunityRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityMembershipRows_ParticipantRows_ParticipantId",
                table: "CommunityMembershipRows",
                column: "ParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityNodeRows_CommunityRows_CommunityId",
                table: "CommunityNodeRows",
                column: "CommunityId",
                principalTable: "CommunityRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityNodeRows_NodeRows_NodeId",
                table: "CommunityNodeRows",
                column: "NodeId",
                principalTable: "NodeRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityNodeRows_ParticipantRows_AssociatedByParticipantId",
                table: "CommunityNodeRows",
                column: "AssociatedByParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CommunityRows_ParticipantRows_OwnerParticipantId",
                table: "CommunityRows",
                column: "OwnerParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentBlocks_BlockRows_BlockId",
                table: "DocumentBlocks",
                column: "BlockId",
                principalTable: "BlockRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationCaseRows_NodeRows_NodeId",
                table: "ModerationCaseRows",
                column: "NodeId",
                principalTable: "NodeRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationCaseRows_ParticipantRows_ReporterId",
                table: "ModerationCaseRows",
                column: "ReporterId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationCaseRows_ParticipantRows_RestoredBy",
                table: "ModerationCaseRows",
                column: "RestoredBy",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationCaseRows_ParticipantRows_ReviewerId",
                table: "ModerationCaseRows",
                column: "ReviewerId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeParents_NodeRows_ParentNodeId",
                table: "NodeParents",
                column: "ParentNodeId",
                principalTable: "NodeRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeReactionRows_NodeRows_NodeId",
                table: "NodeReactionRows",
                column: "NodeId",
                principalTable: "NodeRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeReactionRows_ParticipantRows_AppliedByParticipantId",
                table: "NodeReactionRows",
                column: "AppliedByParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeReactionRows_ReactionDefinitionRows_ReactionDefinitionId",
                table: "NodeReactionRows",
                column: "ReactionDefinitionId",
                principalTable: "ReactionDefinitionRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeRequestedTypes_NodeTypeRows_TypeId",
                table: "NodeRequestedTypes",
                column: "TypeId",
                principalTable: "NodeTypeRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeRows_DocumentRows_DescriptionId",
                table: "NodeRows",
                column: "DescriptionId",
                principalTable: "DocumentRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeRows_NodeTypeRows_TypeId",
                table: "NodeRows",
                column: "TypeId",
                principalTable: "NodeTypeRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NodeRows_ParticipantRows_AuthorId",
                table: "NodeRows",
                column: "AuthorId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationPreferencesRows_ParticipantRows_ParticipantId",
                table: "NotificationPreferencesRows",
                column: "ParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRows_ParticipantRows_ActorParticipantId",
                table: "NotificationRows",
                column: "ActorParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NotificationRows_ParticipantRows_RecipientParticipantId",
                table: "NotificationRows",
                column: "RecipientParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionAuditEntries_NodeReactionRows_RelatedNodeReactionId",
                table: "ReactionAuditEntries",
                column: "RelatedNodeReactionId",
                principalTable: "NodeReactionRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionAuditEntries_ParticipantRows_ActorParticipantId",
                table: "ReactionAuditEntries",
                column: "ActorParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReactionDefinitionRows_ParticipantRows_CreatedByParticipantId",
                table: "ReactionDefinitionRows",
                column: "CreatedByParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VoteRows_ParticipantRows_ParticipantId",
                table: "VoteRows",
                column: "ParticipantId",
                principalTable: "ParticipantRows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentRows_CommentRows_ParentCommentId",
                table: "CommentRows");

            migrationBuilder.DropForeignKey(
                name: "FK_CommentRows_ParticipantRows_AuthorParticipantId",
                table: "CommentRows");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityMembershipRows_CommunityRows_CommunityId",
                table: "CommunityMembershipRows");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityMembershipRows_ParticipantRows_ParticipantId",
                table: "CommunityMembershipRows");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityNodeRows_CommunityRows_CommunityId",
                table: "CommunityNodeRows");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityNodeRows_NodeRows_NodeId",
                table: "CommunityNodeRows");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityNodeRows_ParticipantRows_AssociatedByParticipantId",
                table: "CommunityNodeRows");

            migrationBuilder.DropForeignKey(
                name: "FK_CommunityRows_ParticipantRows_OwnerParticipantId",
                table: "CommunityRows");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentBlocks_BlockRows_BlockId",
                table: "DocumentBlocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationCaseRows_NodeRows_NodeId",
                table: "ModerationCaseRows");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationCaseRows_ParticipantRows_ReporterId",
                table: "ModerationCaseRows");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationCaseRows_ParticipantRows_RestoredBy",
                table: "ModerationCaseRows");

            migrationBuilder.DropForeignKey(
                name: "FK_ModerationCaseRows_ParticipantRows_ReviewerId",
                table: "ModerationCaseRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeParents_NodeRows_ParentNodeId",
                table: "NodeParents");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeReactionRows_NodeRows_NodeId",
                table: "NodeReactionRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeReactionRows_ParticipantRows_AppliedByParticipantId",
                table: "NodeReactionRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeReactionRows_ReactionDefinitionRows_ReactionDefinitionId",
                table: "NodeReactionRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeRequestedTypes_NodeTypeRows_TypeId",
                table: "NodeRequestedTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeRows_DocumentRows_DescriptionId",
                table: "NodeRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeRows_NodeTypeRows_TypeId",
                table: "NodeRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NodeRows_ParticipantRows_AuthorId",
                table: "NodeRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationPreferencesRows_ParticipantRows_ParticipantId",
                table: "NotificationPreferencesRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRows_ParticipantRows_ActorParticipantId",
                table: "NotificationRows");

            migrationBuilder.DropForeignKey(
                name: "FK_NotificationRows_ParticipantRows_RecipientParticipantId",
                table: "NotificationRows");

            migrationBuilder.DropForeignKey(
                name: "FK_ReactionAuditEntries_NodeReactionRows_RelatedNodeReactionId",
                table: "ReactionAuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReactionAuditEntries_ParticipantRows_ActorParticipantId",
                table: "ReactionAuditEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ReactionDefinitionRows_ParticipantRows_CreatedByParticipantId",
                table: "ReactionDefinitionRows");

            migrationBuilder.DropForeignKey(
                name: "FK_VoteRows_ParticipantRows_ParticipantId",
                table: "VoteRows");

            migrationBuilder.DropIndex(
                name: "IX_VoteRows_ParticipantId",
                table: "VoteRows");

            migrationBuilder.DropIndex(
                name: "IX_ReactionDefinitionRows_CreatedByParticipantId",
                table: "ReactionDefinitionRows");

            migrationBuilder.DropIndex(
                name: "IX_ReactionAuditEntries_ActorParticipantId",
                table: "ReactionAuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_ReactionAuditEntries_RelatedNodeReactionId",
                table: "ReactionAuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRows_ActorParticipantId",
                table: "NotificationRows");

            migrationBuilder.DropIndex(
                name: "IX_NotificationRows_RecipientParticipantId",
                table: "NotificationRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeRows_AuthorId",
                table: "NodeRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeRows_DescriptionId",
                table: "NodeRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeRows_TypeId",
                table: "NodeRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeRequestedTypes_TypeId",
                table: "NodeRequestedTypes");

            migrationBuilder.DropIndex(
                name: "IX_NodeReactionRows_AppliedByParticipantId",
                table: "NodeReactionRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeReactionRows_NodeId",
                table: "NodeReactionRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeReactionRows_ReactionDefinitionId",
                table: "NodeReactionRows");

            migrationBuilder.DropIndex(
                name: "IX_NodeParents_ParentNodeId",
                table: "NodeParents");

            migrationBuilder.DropIndex(
                name: "IX_ModerationCaseRows_NodeId",
                table: "ModerationCaseRows");

            migrationBuilder.DropIndex(
                name: "IX_ModerationCaseRows_ReporterId",
                table: "ModerationCaseRows");

            migrationBuilder.DropIndex(
                name: "IX_ModerationCaseRows_RestoredBy",
                table: "ModerationCaseRows");

            migrationBuilder.DropIndex(
                name: "IX_ModerationCaseRows_ReviewerId",
                table: "ModerationCaseRows");

            migrationBuilder.DropIndex(
                name: "IX_DocumentBlocks_BlockId",
                table: "DocumentBlocks");

            migrationBuilder.DropIndex(
                name: "IX_CommunityRows_OwnerParticipantId",
                table: "CommunityRows");

            migrationBuilder.DropIndex(
                name: "IX_CommunityNodeRows_AssociatedByParticipantId",
                table: "CommunityNodeRows");

            migrationBuilder.DropIndex(
                name: "IX_CommunityNodeRows_NodeId",
                table: "CommunityNodeRows");

            migrationBuilder.DropIndex(
                name: "IX_CommunityMembershipRows_ParticipantId",
                table: "CommunityMembershipRows");

            migrationBuilder.DropIndex(
                name: "IX_CommentRows_AuthorParticipantId",
                table: "CommentRows");

            migrationBuilder.DropIndex(
                name: "IX_CommentRows_ParentCommentId",
                table: "CommentRows");
        }
    }
}
