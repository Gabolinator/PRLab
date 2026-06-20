using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRLab.Infrastructure.DB.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Descriptions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultLanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeedHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeedHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Audit_IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutBlock",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BlockType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RepeatCount = table.Column<int>(type: "integer", nullable: false),
                    PrepareTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    RestBetweenRepeatsPolicy = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RestBetweenRepeatsSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestBetweenRepeatsMinimumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestBetweenRepeatsMaximumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterBlockPolicy = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RestAfterBlockSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterBlockMinimumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterBlockMaximumSeconds = table.Column<int>(type: "integer", nullable: true),
                    EstimatedRepeatDurationExpected = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EstimatedRepeatDurationMinimum = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EstimatedRepeatDurationMaximum = table.Column<TimeSpan>(type: "interval", nullable: true),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutBlock", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DescriptionTranslations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescriptionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DescriptionTranslations_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exercise",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataOrigin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercise", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercise_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BaseMovementCategory = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DataOrigin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovementCategory_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Muscle",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LatinName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BodySection = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Muscle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Muscle_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workout",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EstimatedDurationExpected = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EstimatedDurationMinimum = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EstimatedDurationMaximum = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataOrigin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workout", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workout_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutBlockSegment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutBlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    WorkMode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Intent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IntentTargetIntensityType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IntentTargetIntensityValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    IntentTargetIntensityRangeMinValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    IntentTargetIntensityRangeMaxValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    IntentTargetIntensityPaceUnit = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IntentTargetIntensityPaceDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ScoreType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TimeConstraintKind = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    TimeConstraintDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IntervalDuration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IntervalScope = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IntervalStartsOnClock = table.Column<bool>(type: "boolean", nullable: true),
                    EstimatedSegmentDurationExpected = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EstimatedSegmentDurationMinimum = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EstimatedSegmentDurationMaximum = table.Column<TimeSpan>(type: "interval", nullable: true),
                    RestAfterStepPolicy = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RestAfterStepSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterStepMinimumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterStepMaximumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterSegmentPolicy = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RestAfterSegmentSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterSegmentMinimumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestAfterSegmentMaximumSeconds = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutBlockSegment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutBlockSegment_WorkoutBlock_WorkoutBlockId",
                        column: x => x.WorkoutBlockId,
                        principalSchema: "public",
                        principalTable: "WorkoutBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NameKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MovementCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Laterality = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    VariantOfId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryPattern = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    DefaultWorkTargetType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataOrigin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    OwnerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Audit_CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Audit_CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Audit_UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Audit_DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Audit_DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movement_Descriptions_DescriptionId",
                        column: x => x.DescriptionId,
                        principalSchema: "public",
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movement_MovementCategory_MovementCategoryId",
                        column: x => x.MovementCategoryId,
                        principalSchema: "public",
                        principalTable: "MovementCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movement_Movement_VariantOfId",
                        column: x => x.VariantOfId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MuscleAntagonist",
                schema: "public",
                columns: table => new
                {
                    MuscleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AntagonistMuscleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleAntagonist", x => new { x.MuscleId, x.AntagonistMuscleId });
                    table.ForeignKey(
                        name: "FK_MuscleAntagonist_Muscle_AntagonistMuscleId",
                        column: x => x.AntagonistMuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MuscleAntagonist_Muscle_MuscleId",
                        column: x => x.MuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutBlockAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutBlockId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutBlockAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutBlockAssignment_WorkoutBlock_WorkoutBlockId",
                        column: x => x.WorkoutBlockId,
                        principalSchema: "public",
                        principalTable: "WorkoutBlock",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutBlockAssignment_Workout_WorkoutId",
                        column: x => x.WorkoutId,
                        principalSchema: "public",
                        principalTable: "Workout",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutBlockSegmentStep",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    StepKind = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    RestPolicy = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    RestSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestMinimumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestMaximumSeconds = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Prescription = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutBlockSegmentStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutBlockSegmentStep_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "public",
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkoutBlockSegmentStep_WorkoutBlockSegment_SegmentId",
                        column: x => x.SegmentId,
                        principalSchema: "public",
                        principalTable: "WorkoutBlockSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseBlock",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Target_Value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Target_TargetType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Target_Scope = table.Column<int>(type: "integer", nullable: false),
                    LoadTarget_Value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    LoadTarget_Type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    LoadTarget_Unit = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    LoadTarget_LoadReference_Kind = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    LoadTarget_LoadReference_ExerciseId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoadTarget_LoadReference_MovementId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoadTarget_LoadReference_Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LoadTarget_ReferenceRepMax = table.Column<int>(type: "integer", nullable: true),
                    RestBetweenReps_Policy = table.Column<int>(type: "integer", nullable: false),
                    RestBetweenReps_Seconds = table.Column<int>(type: "integer", nullable: true),
                    RestBetweenReps_MinimumSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestBetweenReps_MaximumSeconds = table.Column<int>(type: "integer", nullable: true),
                    TransitionAfterStep_Policy = table.Column<int>(type: "integer", nullable: false),
                    TransitionAfterStep_Seconds = table.Column<int>(type: "integer", nullable: true),
                    TransitionAfterStep_MinimumSeconds = table.Column<int>(type: "integer", nullable: true),
                    TransitionAfterStep_MaximumSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_EccentricSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_BottomPauseSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_ConcentricSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_TopPauseSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExecutionDetails_EccentricIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_BottomIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_ConcentricIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_TopIntent = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ExecutionDetails_Intent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseBlock", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseBlock_Exercise_ExerciseId",
                        column: x => x.ExerciseId,
                        principalSchema: "public",
                        principalTable: "Exercise",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseBlock_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementAllowedWorkTarget",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementAllowedWorkTarget", x => new { x.MovementId, x.TargetType });
                    table.ForeignKey(
                        name: "FK_MovementAllowedWorkTarget_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovementEquipmentRequirement",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Kind = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementEquipmentRequirement", x => new { x.MovementId, x.EquipmentId, x.GroupKey, x.Kind });
                    table.ForeignKey(
                        name: "FK_MovementEquipmentRequirement_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalSchema: "public",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovementEquipmentRequirement_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovementMuscle",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementMuscle", x => new { x.MovementId, x.MuscleId });
                    table.ForeignKey(
                        name: "FK_MovementMuscle_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovementMuscle_Muscle_MuscleId",
                        column: x => x.MuscleId,
                        principalSchema: "public",
                        principalTable: "Muscle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovementPatternTag",
                schema: "public",
                columns: table => new
                {
                    MovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pattern = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementPatternTag", x => new { x.MovementId, x.Pattern });
                    table.ForeignKey(
                        name: "FK_MovementPatternTag_Movement_MovementId",
                        column: x => x.MovementId,
                        principalSchema: "public",
                        principalTable: "Movement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Descriptions_DefaultLanguageCode",
                schema: "public",
                table: "Descriptions",
                column: "DefaultLanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionTranslations_DescriptionId",
                schema: "public",
                table: "DescriptionTranslations",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionTranslations_DescriptionId_LanguageCode",
                schema: "public",
                table: "DescriptionTranslations",
                columns: new[] { "DescriptionId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DescriptionTranslations_LanguageCode_Content",
                schema: "public",
                table: "DescriptionTranslations",
                columns: new[] { "LanguageCode", "Content" });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_DataOrigin",
                schema: "public",
                table: "Equipment",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_DescriptionId",
                schema: "public",
                table: "Equipment",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_IsDeleted",
                schema: "public",
                table: "Equipment",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Name",
                schema: "public",
                table: "Equipment",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_NameKey",
                schema: "public",
                table: "Equipment",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_OwnerUserId",
                schema: "public",
                table: "Equipment",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_DataOrigin",
                schema: "public",
                table: "Exercise",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_DescriptionId",
                schema: "public",
                table: "Exercise",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_IsDeleted",
                schema: "public",
                table: "Exercise",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_Name",
                schema: "public",
                table: "Exercise",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_NameKey",
                schema: "public",
                table: "Exercise",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Exercise_OwnerUserId",
                schema: "public",
                table: "Exercise",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBlock_ExerciseId",
                schema: "public",
                table: "ExerciseBlock",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBlock_ExerciseId_Sequence",
                schema: "public",
                table: "ExerciseBlock",
                columns: new[] { "ExerciseId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBlock_MovementId",
                schema: "public",
                table: "ExerciseBlock",
                column: "MovementId");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_DataOrigin",
                schema: "public",
                table: "Movement",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_DefaultWorkTargetType",
                schema: "public",
                table: "Movement",
                column: "DefaultWorkTargetType");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_DescriptionId",
                schema: "public",
                table: "Movement",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_IsDeleted",
                schema: "public",
                table: "Movement",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_Laterality",
                schema: "public",
                table: "Movement",
                column: "Laterality");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_MovementCategoryId",
                schema: "public",
                table: "Movement",
                column: "MovementCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_Name",
                schema: "public",
                table: "Movement",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_NameKey",
                schema: "public",
                table: "Movement",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_OwnerUserId",
                schema: "public",
                table: "Movement",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_PrimaryPattern",
                schema: "public",
                table: "Movement",
                column: "PrimaryPattern");

            migrationBuilder.CreateIndex(
                name: "IX_Movement_VariantOfId",
                schema: "public",
                table: "Movement",
                column: "VariantOfId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementAllowedWorkTarget_TargetType",
                schema: "public",
                table: "MovementAllowedWorkTarget",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_BaseMovementCategory",
                schema: "public",
                table: "MovementCategory",
                column: "BaseMovementCategory");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_DataOrigin",
                schema: "public",
                table: "MovementCategory",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_DescriptionId",
                schema: "public",
                table: "MovementCategory",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_IsDeleted",
                schema: "public",
                table: "MovementCategory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_Name",
                schema: "public",
                table: "MovementCategory",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_NameKey",
                schema: "public",
                table: "MovementCategory",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MovementCategory_OwnerUserId",
                schema: "public",
                table: "MovementCategory",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementEquipmentRequirement_EquipmentId",
                schema: "public",
                table: "MovementEquipmentRequirement",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementMuscle_MuscleId",
                schema: "public",
                table: "MovementMuscle",
                column: "MuscleId");

            migrationBuilder.CreateIndex(
                name: "IX_MovementMuscle_Role",
                schema: "public",
                table: "MovementMuscle",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_MovementPatternTag_Pattern",
                schema: "public",
                table: "MovementPatternTag",
                column: "Pattern");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_BodySection",
                schema: "public",
                table: "Muscle",
                column: "BodySection");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_DescriptionId",
                schema: "public",
                table: "Muscle",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_IsDeleted",
                schema: "public",
                table: "Muscle",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_LatinName",
                schema: "public",
                table: "Muscle",
                column: "LatinName");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_Name",
                schema: "public",
                table: "Muscle",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Muscle_NameKey",
                schema: "public",
                table: "Muscle",
                column: "NameKey",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MuscleAntagonist_AntagonistMuscleId",
                schema: "public",
                table: "MuscleAntagonist",
                column: "AntagonistMuscleId");

            migrationBuilder.CreateIndex(
                name: "IX_SeedHistory_Name_Version",
                schema: "public",
                table: "SeedHistory",
                columns: new[] { "Name", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Audit_IsDeleted",
                schema: "public",
                table: "Users",
                column: "Audit_IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                schema: "public",
                table: "Users",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Role",
                schema: "public",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_DataOrigin",
                schema: "public",
                table: "Workout",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_DescriptionId",
                schema: "public",
                table: "Workout",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_IsDeleted",
                schema: "public",
                table: "Workout",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_NameKey",
                schema: "public",
                table: "Workout",
                column: "NameKey");

            migrationBuilder.CreateIndex(
                name: "IX_Workout_OwnerUserId",
                schema: "public",
                table: "Workout",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlock_BlockType",
                schema: "public",
                table: "WorkoutBlock",
                column: "BlockType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlock_DataOrigin",
                schema: "public",
                table: "WorkoutBlock",
                column: "DataOrigin");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlock_IsDeleted",
                schema: "public",
                table: "WorkoutBlock",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlock_NameKey",
                schema: "public",
                table: "WorkoutBlock",
                column: "NameKey");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlock_OwnerUserId",
                schema: "public",
                table: "WorkoutBlock",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockAssignment_WorkoutBlockId",
                schema: "public",
                table: "WorkoutBlockAssignment",
                column: "WorkoutBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockAssignment_WorkoutId",
                schema: "public",
                table: "WorkoutBlockAssignment",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockAssignment_WorkoutId_Sequence",
                schema: "public",
                table: "WorkoutBlockAssignment",
                columns: new[] { "WorkoutId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegment_ScoreType",
                schema: "public",
                table: "WorkoutBlockSegment",
                column: "ScoreType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegment_WorkMode",
                schema: "public",
                table: "WorkoutBlockSegment",
                column: "WorkMode");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegment_WorkoutBlockId",
                schema: "public",
                table: "WorkoutBlockSegment",
                column: "WorkoutBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegment_WorkoutBlockId_Sequence",
                schema: "public",
                table: "WorkoutBlockSegment",
                columns: new[] { "WorkoutBlockId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegmentStep_ExerciseId",
                schema: "public",
                table: "WorkoutBlockSegmentStep",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegmentStep_SegmentId",
                schema: "public",
                table: "WorkoutBlockSegmentStep",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegmentStep_SegmentId_Sequence",
                schema: "public",
                table: "WorkoutBlockSegmentStep",
                columns: new[] { "SegmentId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutBlockSegmentStep_StepKind",
                schema: "public",
                table: "WorkoutBlockSegmentStep",
                column: "StepKind");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DescriptionTranslations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExerciseBlock",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementAllowedWorkTarget",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementEquipmentRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementMuscle",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementPatternTag",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MuscleAntagonist",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SeedHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkoutBlockAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkoutBlockSegmentStep",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Equipment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Movement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Muscle",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Workout",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Exercise",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkoutBlockSegment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MovementCategory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WorkoutBlock",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Descriptions",
                schema: "public");
        }
    }
}
