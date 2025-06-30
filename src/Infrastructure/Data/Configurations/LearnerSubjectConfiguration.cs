using Lisa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lisa.Infrastructure.Data.Configurations;

public class LearnerSubjectConfiguration : IEntityTypeConfiguration<LearnerSubject>
{
    public void Configure(EntityTypeBuilder<LearnerSubject> builder)
    {
        builder.HasKey(e => new { e.LearnerId, e.SubjectId });

        builder.HasOne(e => e.Learner)
            .WithMany(l => l.LearnerSubjects)
            .HasForeignKey(e => e.LearnerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Subject)
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Combination)
            .WithMany()
            .HasForeignKey(e => e.CombinationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
