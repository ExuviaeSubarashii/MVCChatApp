using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MVC.Domain.Models
{
    public partial class ChatContext : DbContext
    {
        public ChatContext()
        {
        }

        public ChatContext(DbContextOptions<ChatContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Server> Servers { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<DirectMessages> DirectMessages { get; set; }= null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-12NGJ7T;Initial Catalog=Chat;Integrated Security=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Channel)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ImageDir)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Message1)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .HasColumnName("Message")
                    .IsFixedLength();

                entity.Property(e => e.SenderName)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .HasColumnName("Sender_Name")
                    .IsFixedLength();

                entity.Property(e => e.SenderTime)
                    .HasColumnType("datetime")
                    .HasColumnName("Sender_Time");

                entity.Property(e => e.Server)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<Server>(entity =>
            {
                entity.Property(e => e.Channels)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ServerName)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Usernames)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.ServerOwner)
                   .HasMaxLength(310)
                   .IsUnicode(false)
                   .IsFixedLength();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.HasPassword)
                    .HasMaxLength(32)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Image).HasColumnType("image");

                entity.Property(e => e.Password)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.Server)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Username)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.Friends)
                    .HasMaxLength(25)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.FriendRequests)
                    .HasMaxLength(2000)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.BlockedUsers)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.EMail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.StatusMessage)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.UserId)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength();
                entity.Property(e => e.CreationDate)
                    .HasColumnType("datetime");
            });
            modelBuilder.Entity<DirectMessages>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("Id");

                entity.Property(e => e.SenderName)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.ReceiverName)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Message)
                    .HasMaxLength(310)
                    .IsUnicode(false)
                    .HasColumnName("Message")
                    .IsFixedLength();

                entity.Property(e => e.MessageDate)
                    .HasColumnType("datetime");
            });
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
