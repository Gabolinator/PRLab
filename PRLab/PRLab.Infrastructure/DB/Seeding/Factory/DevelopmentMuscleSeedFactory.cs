using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Infrastructure.DB.Seeding;

namespace PRLab.Infrastructure.DB.Seeding.Factory;

public sealed class DevelopmentMuscleSeedFactory(IUserService userService) : IMuscleSeedFactory
{
    private User SeedUser => userService.GetAdminUser("Seed");
    
  public IReadOnlyList<SeedItem<Muscle>> CreateInitialData()
{
    var chest = Muscle.New(
        "Chest",
        latinName: "Musculus pectoralis major",
        DomainEnum.BodySection.UpperBody,
        Description.New("Main chest muscle involved in pushing and shoulder adduction."),
        SeedUser
    );

    var lats = Muscle.New(
        "Lats",
        latinName: "Musculus latissimus dorsi",
        DomainEnum.BodySection.UpperBody,
        Description.New("Large back muscle involved in pulling, shoulder extension, and shoulder adduction."),
        SeedUser
    );

    var frontDelts = Muscle.New(
        "Front Delts",
        latinName: "Musculus deltoideus pars clavicularis",
        DomainEnum.BodySection.UpperBody,
        Description.New("Front shoulder muscle involved in shoulder flexion and pressing movements."),
        SeedUser
    );

    var rearDelts = Muscle.New(
        "Rear Delts",
        latinName: "Musculus deltoideus pars spinalis",
        DomainEnum.BodySection.UpperBody,
        Description.New("Rear shoulder muscle involved in shoulder extension and horizontal abduction."),
        SeedUser
    );

    var biceps = Muscle.New(
        "Biceps",
        latinName: "Musculus biceps brachii",
        DomainEnum.BodySection.UpperBody,
        Description.New("Upper arm muscle involved in elbow flexion and forearm supination."),
        SeedUser
    );

    var triceps = Muscle.New(
        "Triceps",
        latinName: "Musculus triceps brachii",
        DomainEnum.BodySection.UpperBody,
        Description.New("Upper arm muscle involved in elbow extension."),
        SeedUser
    );

    var abs = Muscle.New(
        "Abs",
        latinName: "Musculus rectus abdominis",
        DomainEnum.BodySection.MidSection,
        Description.New("Front core muscle involved in trunk flexion and abdominal bracing."),
        SeedUser
    );

    var spinalErectors = Muscle.New(
        "Spinal Erectors",
        latinName: "Musculus erector spinae",
        DomainEnum.BodySection.MidSection,
        Description.New("Back extensor muscle group involved in spinal extension and posture."),
        SeedUser
    );

    var quads = Muscle.New(
        "Quads",
        latinName: "Musculus quadriceps femoris",
        DomainEnum.BodySection.LowerBody,
        Description.New("Front thigh muscle group involved mainly in knee extension."),
        SeedUser
    );

    var hamstrings = Muscle.New(
        "Hamstrings",
        latinName: "Musculi ischiocrurales",
        DomainEnum.BodySection.LowerBody,
        Description.New("Back thigh muscle group involved in knee flexion and hip extension."),
        SeedUser
    );

    var glutes = Muscle.New(
        "Glutes",
        latinName: "Musculus gluteus maximus",
        DomainEnum.BodySection.LowerBody,
        Description.New("Hip extensor muscle group involved in squats, hinges, running, and jumping."),
        SeedUser
    );

    var hipFlexors = Muscle.New(
        "Hip Flexors",
        latinName: "Musculus iliopsoas",
        DomainEnum.BodySection.LowerBody,
        Description.New("Hip flexor muscle group involved in lifting the thigh and flexing the hip."),
        SeedUser
    );

    var calves = Muscle.New(
        "Calves",
        latinName: "Musculus gastrocnemius",
        DomainEnum.BodySection.LowerBody,
        Description.New("Lower-leg muscle group involved in plantar flexion and jumping."),
        SeedUser
    );

    var shins = Muscle.New(
        "Shins",
        latinName: "Musculus tibialis anterior",
        DomainEnum.BodySection.LowerBody,
        Description.New("Front lower-leg muscle involved in dorsiflexion of the foot."),
        SeedUser
    );

    AddAntagonistPair(chest, lats);
    AddAntagonistPair(frontDelts, rearDelts);
    AddAntagonistPair(biceps, triceps);
    AddAntagonistPair(abs, spinalErectors);
    AddAntagonistPair(quads, hamstrings);
    AddAntagonistPair(glutes, hipFlexors);
    AddAntagonistPair(calves, shins);

    var muscles =
        new List<Muscle>
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
            new SeedItem<Muscle>(
                SeedKeyGenerator.GenerateMuscleKey(muscle),
                muscle))
        .ToList();
}

private static void AddAntagonistPair(Muscle firstMuscle, Muscle secondMuscle)
{
    firstMuscle.AddAntagonist(secondMuscle.Id);
    secondMuscle.AddAntagonist(firstMuscle.Id);
}
}