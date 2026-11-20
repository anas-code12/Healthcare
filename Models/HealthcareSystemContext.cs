using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Healthcare.Models;

public partial class HealthcareSystemContext : DbContext
{
    public HealthcareSystemContext()
    {
    }

    public HealthcareSystemContext(DbContextOptions<HealthcareSystemContext> options)
        : base(options)
    {
    }
    public virtual DbSet<DoctorMessage> DoctorMessages { get; set; }
    public virtual DbSet<Admin> Admins { get; set; }
    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<Clinic> Clinics { get; set; }
    public virtual DbSet<ClinicBreak> ClinicBreaks { get; set; }
    public virtual DbSet<Disease> Diseases { get; set; }
    public virtual DbSet<Doctor> Doctors { get; set; }
    public virtual DbSet<Medicine> Medicines { get; set; }  // Added DbSet for Medicine
    public virtual DbSet<MinistryOfHealth> MinistryOfHealths { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<PatientSickRecord> PatientSickRecords { get; set; }
    public virtual DbSet<Specialty> Specialties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PK__admin__43AA414142DF4F26");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PK__appointm__A50828FC7783888A");

            entity.HasOne(d => d.Clinic).WithMany(p => p.Appointments)
                .HasConstraintName("FK__appointme__clini__4BAC3F29");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__appointme__patie__49C3F6B7");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_appointment_doctor");

     
        });

        modelBuilder.Entity<Clinic>(entity =>
        {
            entity.HasKey(e => e.ClinicId).HasName("PK__clinics__A0C8D19B1A5488F6");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Clinics)
                .HasConstraintName("FK__clinics__doctor___4AB81AF0");

            entity.Property(e => e.SlotDurationMinutes).HasDefaultValue(30);
        });

        modelBuilder.Entity<ClinicBreak>(entity =>
        {
            entity.HasKey(e => e.BreakId);

            entity.HasOne(d => d.Clinic).WithMany(p => p.Breaks)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_breaks_clinic");

            entity.HasIndex(e => new { e.ClinicId, e.BreakTime })
                .IsUnique()
                .HasDatabaseName("UQ_break");
        });

        modelBuilder.Entity<Disease>(entity =>
        {
            entity.HasKey(e => e.DiseaseId).HasName("PK__diseases__15627065EF557461");

            entity.HasOne(d => d.Specialty).WithMany(p => p.Diseases)
                .HasForeignKey(d => d.SpecialtyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__diseases__specia__48CFD27E");
        });

        // Medicine table configuration
        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.ToTable("medicine");

            entity.HasKey(e => e.MedicineId);

            entity.Property(e => e.MedicineId)
                .HasColumnName("medicine_id");

            entity.Property(e => e.MedicineName)
                .HasColumnName("medicine_name")
                .HasMaxLength(100);

            entity.Property(e => e.SpecialtyId)
                .HasColumnName("specialty_id");

            entity.HasOne(e => e.Specialty)
                .WithMany()
                .HasForeignKey(e => e.SpecialtyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MinistryOfHealth>(entity =>
        {
            entity.HasKey(e => e.MinistryOfHealthId).HasName("PK__Ministry__56640D50E3C40186");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__patients__4D5CE476FA5A7DE2");

            entity.Property(e => e.PatientGender).IsFixedLength();
        });

        modelBuilder.Entity<PatientSickRecord>(entity =>
        {
            entity.HasKey(e => e.SickRecordId).HasName("PK__patient___E4769CA0E5537975");

            entity.HasOne(d => d.Patient).WithOne(p => p.PatientSickRecord)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__patient_S__patie__47DBAE45");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.SpecialtyId).HasName("PK__specialt__B90D8D1294424308");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}