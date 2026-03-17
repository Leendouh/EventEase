using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace EventEase.Models
{
    public class Admin
    {
        [Key] // The key attribute that indicates this property as the primary key of this admin entity
        public int AdminId { get; set; } // The unique identifier of each admin

        // COMPUTED PROPERTY for formatted Admin ID (e.g., A001)
        [Display(Name = "Admin ID")]
        public string FormattedAdminId => "A" + AdminId.ToString("D3");

        [Required] // The required attribute that indicates that this property must have a value and cannot be null
        [StringLength(50)] // Set a string length of 50 characters
        public string Username { get; set; } = string.Empty; // The username of each admin

        [Required] // The required attribute that indicates that this property must have a value and cannot be null
        public string PasswordHash { get; set; } = string.Empty; // The hashed password of each user (stored as salt:hash)

        [Required] // The required attribute that indicates that this property must have a value and cannot be null
        [Display(Name = "Full Name")] // The full name display to the user interface
        [StringLength(100)] // Specify the length of the full name not to exceed 100 
        public string FullName { get; set; } = string.Empty; // The full name of the admin

        [Required] // The required attribute that indicates that this property must have a value and cannot be null
        [EmailAddress] // Calling the email address attributes and their functions
        [StringLength(100)] // Specify the length of the email address not to exceed 100
        public string Email { get; set; } = string.Empty; // The email address of the admin

        [Required] // The required attribute that indicates that this property must have a value and cannot be null
        [StringLength(20)] // Specify the length of the admin role not to exceed 20 
        public string Role { get; set; } = string.Empty; // "SuperAdmin" or "BookingSpecialist"

        [Display(Name = "Active")] // The default "Active" status of the admin
        public bool IsActive { get; set; } = true; // The active status of the admin

        [Display(Name = "Created Date")] // Display the created date to the user interface
        [DataType(DataType.DateTime)] // Put the data type for the date and time of the created admin
        public DateTime CreatedDate { get; set; } = DateTime.Now; // The created date for the new admin

        [Display(Name = "Last Login")] // Display the last login date and time of the admin
        [DataType(DataType.DateTime)] // The data type of the last login for the admin
        public DateTime? LastLoginDate { get; set; } // The date and time of the last login

        [Display(Name = "Phone Number")] // The phone numbers for the admin displayed on the user interface
        [Phone] // Phone attribute features that are for phone numbers
        [StringLength(10)] // The length of the 10 phone number digits
        public string? PhoneNumber { get; set; } = null; // The phone numbers of the admin

        // Flag for first-time login
        [Display(Name = "First Time Login")]
        public bool IsFirstLogin { get; set; } = true;

        // Track password changes
        [Display(Name = "Last Password Change")]
        public DateTime? LastPasswordChangeDate { get; set; }

        // Navigation property for creator
        public int? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual Admin? CreatedByAdmin { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
        public virtual ICollection<Venue>? Venues { get; set; }
        public virtual ICollection<Event>? Events { get; set; }

        /// <summary>
        /// Creates a password hash in the format "salt:hash" using a GUID salt (matches SQL NEWID())
        /// </summary>
        public static string HashPassword(string password)
        {
            // Generate a random GUID for salt (matches SQL NEWID() format)
            string salt = Guid.NewGuid().ToString().ToLower();

            // Combine password with salt
            string saltedPassword = password + salt;

            // Hash with SHA256
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                // Convert to hex string (lowercase, no hyphens)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                string hash = builder.ToString();

                // Return salt:hash format
                return salt + ":" + hash;
            }
        }

        /// <summary>
        /// Verifies a password against the stored hash (supports salt:hash format)
        /// </summary>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(PasswordHash) || !PasswordHash.Contains(':'))
                return false;

            try
            {
                // Split the stored value into salt and hash
                var parts = PasswordHash.Split(':');
                if (parts.Length != 2)
                    return false;

                string salt = parts[0];      // This is the GUID with hyphens
                string storedHash = parts[1]; // This is the 64-char hex hash

                // Combine password with salt
                string saltedPassword = password + salt;

                // Hash with SHA256
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                    // Convert to hex string (lowercase, no hyphens)
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    string computedHash = builder.ToString();

                    // Compare (case-insensitive)
                    return computedHash.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Changes the user's password to a new one
        /// </summary>
        public void ChangePassword(string newPassword)
        {
            PasswordHash = HashPassword(newPassword);
            IsFirstLogin = false;
            LastPasswordChangeDate = DateTime.Now;
        }

        /// <summary>
        /// Generates a random temporary password for new sub-admins
        /// </summary>
        public static (string hash, string plainPassword) GenerateTemporaryPassword()
        {
            // Generate a random temporary password (8 characters)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            string tempPassword = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return (HashPassword(tempPassword), tempPassword);
        }

        /// <summary>
        /// Legacy method for backward compatibility - use HashPassword instead
        /// </summary>
        [Obsolete("Use HashPassword method which includes salt")]
        public static string HashPasswordLegacy(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}