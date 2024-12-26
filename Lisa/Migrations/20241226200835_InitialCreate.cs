using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lisa.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubjectCombinations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectCombinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    SchoolId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grades_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    Administrator_SchoolId = table.Column<int>(type: "integer", nullable: true),
                    Principal_SchoolId = table.Column<int>(type: "integer", nullable: true),
                    SchoolManagement_SchoolId = table.Column<int>(type: "integer", nullable: true),
                    SystemAdministrator_SchoolId = table.Column<int>(type: "integer", nullable: true),
                    SchoolId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Schools_Administrator_SchoolId",
                        column: x => x.Administrator_SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Schools_Principal_SchoolId",
                        column: x => x.Principal_SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SchoolManagement_SchoolId",
                        column: x => x.SchoolManagement_SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Schools_SystemAdministrator_SchoolId",
                        column: x => x.SystemAdministrator_SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubjectCombinationSubjects",
                columns: table => new
                {
                    SubjectCombinationId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectCombinationSubjects", x => new { x.SubjectCombinationId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_SubjectCombinationSubjects_SubjectCombinations_SubjectCombi~",
                        column: x => x.SubjectCombinationId,
                        principalTable: "SubjectCombinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubjectCombinationSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegisterClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    GradeId = table.Column<int>(type: "integer", nullable: false),
                    SubjectCombinationId = table.Column<int>(type: "integer", nullable: false),
                    GradeId1 = table.Column<int>(type: "integer", nullable: true),
                    SchoolId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisterClasses_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegisterClasses_Grades_GradeId1",
                        column: x => x.GradeId1,
                        principalTable: "Grades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegisterClasses_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegisterClasses_SubjectCombinations_SubjectCombinationId",
                        column: x => x.SubjectCombinationId,
                        principalTable: "SubjectCombinations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    SessionStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SessionEndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSubjects",
                columns: table => new
                {
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSubjects", x => new { x.TeacherId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_TeacherSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherSubjects_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Learners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LearnerCode = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegisterClassId = table.Column<int>(type: "integer", nullable: false),
                    RegisterClassId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Learners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Learners_RegisterClasses_RegisterClassId",
                        column: x => x.RegisterClassId,
                        principalTable: "RegisterClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Learners_RegisterClasses_RegisterClassId1",
                        column: x => x.RegisterClassId1,
                        principalTable: "RegisterClasses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionSubjects",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    SessionId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionSubjects", x => new { x.SessionId, x.SubjectId, x.TeacherId });
                    table.ForeignKey(
                        name: "FK_SessionSubjects_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionSubjects_Sessions_SessionId1",
                        column: x => x.SessionId1,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionSubjects_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearnerParents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    CellNumber = table.Column<string>(type: "text", nullable: false),
                    LearnerId = table.Column<int>(type: "integer", nullable: false),
                    LearnerId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerParents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearnerParents_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerParents_Learners_LearnerId1",
                        column: x => x.LearnerId1,
                        principalTable: "Learners",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LearnerId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    ResultDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LearnerId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_Learners_LearnerId",
                        column: x => x.LearnerId,
                        principalTable: "Learners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_Learners_LearnerId1",
                        column: x => x.LearnerId1,
                        principalTable: "Learners",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Results_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Grades_SchoolId",
                table: "Grades",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerParents_Email",
                table: "LearnerParents",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerParents_LearnerId",
                table: "LearnerParents",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerParents_LearnerId1",
                table: "LearnerParents",
                column: "LearnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_LearnerCode",
                table: "Learners",
                column: "LearnerCode");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_RegisterClassId",
                table: "Learners",
                column: "RegisterClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_RegisterClassId1",
                table: "Learners",
                column: "RegisterClassId1");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_GradeId",
                table: "RegisterClasses",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_GradeId1",
                table: "RegisterClasses",
                column: "GradeId1");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_SchoolId",
                table: "RegisterClasses",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterClasses_SubjectCombinationId",
                table: "RegisterClasses",
                column: "SubjectCombinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_LearnerId",
                table: "Results",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_LearnerId1",
                table: "Results",
                column: "LearnerId1");

            migrationBuilder.CreateIndex(
                name: "IX_Results_Score",
                table: "Results",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_Results_SubjectId",
                table: "Results",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TeacherId",
                table: "Sessions",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TeacherId_SessionStartTime_SessionEndTime",
                table: "Sessions",
                columns: new[] { "TeacherId", "SessionStartTime", "SessionEndTime" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionSubjects_SessionId_SubjectId",
                table: "SessionSubjects",
                columns: new[] { "SessionId", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionSubjects_SessionId1",
                table: "SessionSubjects",
                column: "SessionId1");

            migrationBuilder.CreateIndex(
                name: "IX_SessionSubjects_SubjectId",
                table: "SessionSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionSubjects_TeacherId",
                table: "SessionSubjects",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCombinationSubjects_SubjectCombinationId_SubjectId",
                table: "SubjectCombinationSubjects",
                columns: new[] { "SubjectCombinationId", "SubjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_SubjectCombinationSubjects_SubjectId",
                table: "SubjectCombinationSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_SubjectId",
                table: "TeacherSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Administrator_SchoolId",
                table: "Users",
                column: "Administrator_SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastName",
                table: "Users",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Principal_SchoolId",
                table: "Users",
                column: "Principal_SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolId",
                table: "Users",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SchoolManagement_SchoolId",
                table: "Users",
                column: "SchoolManagement_SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SystemAdministrator_SchoolId",
                table: "Users",
                column: "SystemAdministrator_SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearnerParents");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "SessionSubjects");

            migrationBuilder.DropTable(
                name: "SubjectCombinationSubjects");

            migrationBuilder.DropTable(
                name: "TeacherSubjects");

            migrationBuilder.DropTable(
                name: "Learners");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "RegisterClasses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "SubjectCombinations");

            migrationBuilder.DropTable(
                name: "Schools");
        }
    }
}
