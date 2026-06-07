using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory.Muscle;

public sealed class DevelopmentMuscleSeedFactory(IUserService userService) : IMuscleSeedFactory
{
    private User SeedUser => userService.GetAdminUser("Seed");
    
  public IReadOnlyList<SeedItem<Domain.Model.Entity.Muscle>> CreateInitialData()
{
    var chest = Domain.Model.Entity.Muscle.New(
        "Chest",
        latinName: "Musculus pectoralis major",
        DomainEnum.BodySection.UpperBody,
        Description.New("Main chest muscle involved in pushing and shoulder adduction."),
        SeedUser
    );

    var lats = Domain.Model.Entity.Muscle.New(
        "Lats",
        latinName: "Musculus latissimus dorsi",
        DomainEnum.BodySection.UpperBody,
        Description.New("Large back muscle involved in pulling, shoulder extension, and shoulder adduction."),
        SeedUser
    );

    var frontDelts = Domain.Model.Entity.Muscle.New(
        "Front Delts",
        latinName: "Musculus deltoideus pars clavicularis",
        DomainEnum.BodySection.UpperBody,
        Description.New("Front shoulder muscle involved in shoulder flexion and pressing movements."),
        SeedUser
    );

    var rearDelts = Domain.Model.Entity.Muscle.New(
        "Rear Delts",
        latinName: "Musculus deltoideus pars spinalis",
        DomainEnum.BodySection.UpperBody,
        Description.New("Rear shoulder muscle involved in shoulder extension and horizontal abduction."),
        SeedUser
    );

    var biceps = Domain.Model.Entity.Muscle.New(
        "Biceps",
        latinName: "Musculus biceps brachii",
        DomainEnum.BodySection.UpperBody,
        Description.New("Upper arm muscle involved in elbow flexion and forearm supination."),
        SeedUser
    );

    var triceps = Domain.Model.Entity.Muscle.New(
        "Triceps",
        latinName: "Musculus triceps brachii",
        DomainEnum.BodySection.UpperBody,
        Description.New("Upper arm muscle involved in elbow extension."),
        SeedUser
    );

    var abs = Domain.Model.Entity.Muscle.New(
        "Abs",
        latinName: "Musculus rectus abdominis",
        DomainEnum.BodySection.MidSection,
        Description.New("Front core muscle involved in trunk flexion and abdominal bracing."),
        SeedUser
    );

    var spinalErectors = Domain.Model.Entity.Muscle.New(
        "Spinal Erectors",
        latinName: "Musculus erector spinae",
        DomainEnum.BodySection.MidSection,
        Description.New("Back extensor muscle group involved in spinal extension and posture."),
        SeedUser
    );

    var quads = Domain.Model.Entity.Muscle.New(
        "Quads",
        latinName: "Musculus quadriceps femoris",
        DomainEnum.BodySection.LowerBody,
        Description.New("Front thigh muscle group involved mainly in knee extension."),
        SeedUser
    );

    var hamstrings = Domain.Model.Entity.Muscle.New(
        "Hamstrings",
        latinName: "Musculi ischiocrurales",
        DomainEnum.BodySection.LowerBody,
        Description.New("Back thigh muscle group involved in knee flexion and hip extension."),
        SeedUser
    );

    var glutes = Domain.Model.Entity.Muscle.New(
        "Glutes",
        latinName: "Musculus gluteus maximus",
        DomainEnum.BodySection.LowerBody,
        Description.New("Hip extensor muscle group involved in squats, hinges, running, and jumping."),
        SeedUser
    );

    var hipFlexors = Domain.Model.Entity.Muscle.New(
        "Hip Flexors",
        latinName: "Musculus iliopsoas",
        DomainEnum.BodySection.LowerBody,
        Description.New("Hip flexor muscle group involved in lifting the thigh and flexing the hip."),
        SeedUser
    );

    var calves = Domain.Model.Entity.Muscle.New(
        "Calves",
        latinName: "Musculus gastrocnemius",
        DomainEnum.BodySection.LowerBody,
        Description.New("Lower-leg muscle group involved in plantar flexion and jumping."),
        SeedUser
    );

    var shins = Domain.Model.Entity.Muscle.New(
        "Shins",
        latinName: "Musculus tibialis anterior",
        DomainEnum.BodySection.LowerBody,
        Description.New("Front lower-leg muscle involved in dorsiflexion of the foot."),
        SeedUser
    );

    var muscles =
        new List<Domain.Model.Entity.Muscle>
        {
            chest,
            lats,
            frontDelts,
            rearDelts,
            biceps,
            triceps,
            abs,
            spinalErectors,
            quads,
            hamstrings,
            glutes,
            hipFlexors,
            calves,
            shins,
        };

    return muscles
        .Select(muscle =>
            new SeedItem<Domain.Model.Entity.Muscle>(
                SeedKeyGenerator.GenerateMuscleKey(muscle),
                muscle,
                SeedAction.CreateIfMissing))
        .ToList();
}
}