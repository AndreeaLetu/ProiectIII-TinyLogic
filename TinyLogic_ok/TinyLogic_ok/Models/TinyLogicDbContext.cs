using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Emit;
using TinyLogic_ok.Models.LessonModels;
using TinyLogic_ok.Models.CourseModels;
using TinyLogic_ok.Models.TestModels;


namespace TinyLogic_ok.Models
{
    public class TinyLogicDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public TinyLogicDbContext(DbContextOptions<TinyLogicDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<Lessons> Lessons { get; set; }
        public DbSet<Tests> Tests { get; set; }
        public DbSet<UserLessons> UserLessons { get; set; }
        public DbSet<TestProgress> TestProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

          
            modelBuilder.Entity<Courses>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);



            modelBuilder.Entity<Courses>()
                 .HasMany(c => c.Tests)
                 .WithOne(t => t.Course)
                 .HasForeignKey(t => t.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserLessons>()
                 .HasOne(ul => ul.User)
                 .WithMany(u => u.LessonsProgress)
                 .HasForeignKey(ul => ul.UserId);

            modelBuilder.Entity<UserLessons>()
                .HasOne(ul => ul.Lesson)
                .WithMany(l => l.UserProgress)
                .HasForeignKey(ul => ul.LessonId);

            modelBuilder.Entity<Lessons>()
                .Property(l => l.LessonName)
                .IsRequired();

            modelBuilder.Entity<TestProgress>()
                .HasOne(tp => tp.User)
                .WithMany() 
                .HasForeignKey(tp => tp.UserId);

            modelBuilder.Entity<TestProgress>()
                .HasOne(tp => tp.Test)
                .WithMany(t => t.TestProgresses)
                .HasForeignKey(tp => tp.TestId);

        }
    }
}
