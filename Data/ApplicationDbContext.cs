using CinemaProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Cinema_Project.Models; // Add this using directive

namespace CinemaProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Category> Categories { get; set; }
        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieSubImage> MovieSubImages { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<BookingSeat> BookingSeats { get; set; }
        public DbSet<ShowTime> ShowTimes { get; set; }
        public DbSet<PendingSeat> PendingSeats { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Actors)
                .WithMany(a => a.Movies);

            modelBuilder.Entity<ShowTime>()
                .HasOne(st => st.Movie)
                .WithMany(s=>s.ShowTimes)
                .HasForeignKey(st => st.MovieId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ShowTime>()
                .HasOne(st => st.Cinema)
                .WithMany(s=>s.ShowTimes)
                .HasForeignKey(st => st.CinemaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookingSeat>()
                .HasOne(bs => bs.Seat)
                .WithMany(s=>s.BookingSeats)
                .HasForeignKey(bs => bs.SeatId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PendingSeat>()
                        .HasOne(p => p.ShowTime)
                        .WithMany(s => s.PendingSeats)
                        .HasForeignKey(p => p.ShowTimeId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PendingSeat>()
                        .HasOne(p => p.Seat)
                        .WithMany(s => s.PendingSeats)
                        .HasForeignKey(p => p.SeatId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PendingSeat>()
                        .HasOne(p => p.Booking)
                        .WithMany(b => b.PendingSeats)
                        .HasForeignKey(p => p.BookingId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", Description = "Action Movies" },
                new Category { Id = 2, Name = "Drama", Description = "Drama Movies" },
                new Category { Id = 3, Name = "Comedy", Description = "Comedy Movies" }
            );

            modelBuilder.Entity<Cinema>().HasData(
    new Cinema
    {
        Id = 1,
        Name = "Cinema City",
        Description = "Best cinema in town",
        Logo = "cinema1.png",
        Address = "Nasr City, Cairo, Egypt"
    },
    new Cinema
    {
        Id = 2,
        Name = "IMAX Cinema",
        Description = "High quality screens",
        Logo = "cinema2.png",
        Address = "Mall of Egypt, 6th of October, Giza, Egypt"
    },
    new Cinema
    {
        Id = 3,
        Name = "Cairo Cinema",
        Description = "High quality in city",
        Logo = "cinema3.png",
        Address = "Downtown Cairo, Cairo, Egypt"
    }
);
            modelBuilder.Entity<Actor>().HasData(
                new Actor { Id = 1, Name = "Tom Cruise", Image = "Actor1.jpg" },
                new Actor { Id = 2, Name = "Leonardo DiCaprio", Image = "Actor2.jpg" },
                new Actor { Id = 3, Name = "Will Smith", Image = "Actor3.jpg" },
                new Actor { Id = 4, Name = "Robert Downey Jr", Image = "Actor4.jpg" },
                new Actor { Id = 5, Name = "Brad Pitt", Image = "Actor5.jpg" },
                new Actor { Id = 6, Name = "Johnny Depp", Image = "Actor6.jpg" }
            );
            modelBuilder.Entity<Movie>().HasData(
                new Movie
                {
                    Id = 1,
                    Name = "Mission Impossible",
                    Description = "Ethan Hunt faces a new global threat.",
                    Status = "Available",
                    Price = 120,
                    MainImg = "Movie1.jpg",
                    Date = new DateTime(2024, 1, 10),
                    CategoryId = 1,
                    CinemaId = 1
                },
                new Movie
                {
                    Id = 2,
                    Name = "Inception",
                    Description = "A thief who steals corporate secrets through dreams.",
                    Status = "Available",
                    Price = 150,
                    MainImg = "Movie2.jpg",
                    Date = new DateTime(2024, 2, 5),
                    CategoryId = 2,
                    CinemaId = 2
                },
                new Movie
                {
                    Id = 3,
                    Name = "The Dark Knight",
                    Description = "Batman faces the Joker in Gotham City.",
                    Status = "Available",
                    Price = 130,
                    MainImg = "Movie3.jpg",
                    Date = new DateTime(2024, 3, 1),
                    CategoryId = 1,
                    CinemaId = 1
                },
                new Movie
                {
                    Id = 4,
                    Name = "Forrest Gump",
                    Description = "Life story of a simple man with a big heart.",
                    Status = "Available",
                    Price = 100,
                    MainImg = "Movie4.jpg",
                    Date = new DateTime(2024, 4, 12),
                    CategoryId = 2,
                    CinemaId = 2
                },
                new Movie
                {
                    Id = 5,
                    Name = "Rush Hour",
                    Description = "A fast-paced action comedy with two cops.",
                    Status = "Available",
                    Price = 110,
                    MainImg = "Movie5.jpg",
                    Date = new DateTime(2024, 5, 20),
                    CategoryId = 3,
                    CinemaId = 1
                },
                new Movie
                {
                    Id = 6,
                    Name = "Avengers Endgame",
                    Description = "The Avengers assemble for one final battle.",
                    Status = "Available",
                    Price = 160,
                    MainImg = "Movie6.jpg",
                    Date = new DateTime(2024, 6, 1),
                    CategoryId = 1,
                    CinemaId = 2
                },
                new Movie
                {
                    Id = 7,
                    Name = "Titanic",
                    Description = "A tragic love story aboard the Titanic.",
                    Status = "Available",
                    Price = 140,
                    MainImg = "Movie7.jpg",
                    Date = new DateTime(2024, 7, 10),
                    CategoryId = 2,
                    CinemaId = 1
                },
                new Movie
                {
                    Id = 8,
                    Name = "Jumanji",
                    Description = "A magical game turns into a real jungle adventure.",
                    Status = "Available",
                    Price = 115,
                    MainImg = "Movie8.jpg",
                    Date = new DateTime(2024, 8, 5),
                    CategoryId = 3,
                    CinemaId = 2
                },
                new Movie
                {
                    Id = 9,
                    Name = "Gladiator",
                    Description = "A Roman general seeks revenge.",
                    Status = "Available",
                    Price = 125,
                    MainImg = "Movie9.jpg",
                    Date = new DateTime(2024, 9, 15),
                    CategoryId = 1,
                    CinemaId = 1
                },
                new Movie
                {
                    Id = 10,
                    Name = "The Mask",
                    Description = "A man discovers a magical mask.",
                    Status = "Available",
                    Price = 105,
                    MainImg = "Movie10.jpg",
                    Date = new DateTime(2024, 10, 1),
                    CategoryId = 3,
                    CinemaId = 2
                }
            );

            modelBuilder.Entity<MovieSubImage>().HasData(
                new MovieSubImage { Id = 1, SubImg = "sub1.jpg", MovieId = 1 },
                new MovieSubImage { Id = 2, SubImg = "sub2.jpg", MovieId = 1 },
                new MovieSubImage { Id = 3, SubImg = "sub3.jpg", MovieId = 2 }
            );
            modelBuilder.Entity("ActorMovie").HasData(
                // Movie 1
                new { ActorsId = 1, MoviesId = 1 },
                new { ActorsId = 2, MoviesId = 1 },
                new { ActorsId = 4, MoviesId = 1 },

                // Movie 2
                new { ActorsId = 2, MoviesId = 2 },
                new { ActorsId = 3, MoviesId = 2 },
                new { ActorsId = 5, MoviesId = 2 },

                // Movie 3
                new { ActorsId = 1, MoviesId = 3 },
                new { ActorsId = 6, MoviesId = 3 },

                // Movie 4
                new { ActorsId = 2, MoviesId = 4 },
                new { ActorsId = 5, MoviesId = 4 },

                // Movie 5
                new { ActorsId = 3, MoviesId = 5 },
                new { ActorsId = 6, MoviesId = 5 },

                // Movie 6
                new { ActorsId = 1, MoviesId = 6 },
                new { ActorsId = 3, MoviesId = 6 },
                new { ActorsId = 4, MoviesId = 6 },

                // Movie 7
                new { ActorsId = 2, MoviesId = 7 },
                new { ActorsId = 5, MoviesId = 7 },

                // Movie 8
                new { ActorsId = 3, MoviesId = 8 },
                new { ActorsId = 6, MoviesId = 8 },

                // Movie 9
                new { ActorsId = 1, MoviesId = 9 },
                new { ActorsId = 4, MoviesId = 9 },

                // Movie 10
                new { ActorsId = 2, MoviesId = 10 },
                new { ActorsId = 3, MoviesId = 10 },
                new { ActorsId = 6, MoviesId = 10 }
            );
        }
    }
}
