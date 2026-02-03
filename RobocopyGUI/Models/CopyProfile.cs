using System;
using System.IO;
using System.Text.Json;

namespace RobocopyGUI.Models
{
    /// <summary>
    /// Represents a saved copy profile that stores copy options for reuse.
    /// </summary>
    public class CopyProfile
    {
        /// <summary>
        /// Gets or sets the profile name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the profile description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the copy options associated with this profile.
        /// </summary>
        public CopyOptions Options { get; set; } = new CopyOptions();

        /// <summary>
        /// Gets or sets when the profile was created.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets when the profile was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets when the profile was last used.
        /// </summary>
        public DateTime? LastUsedDate { get; set; }

        /// <summary>
        /// Saves the profile to a JSON file.
        /// </summary>
        /// <param name="filePath">The path to save the profile to.</param>
        public void Save(string filePath)
        {
            LastModifiedDate = DateTime.Now;
            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Loads a profile from a JSON file.
        /// </summary>
        /// <param name="filePath">The path to load the profile from.</param>
        /// <returns>The loaded CopyProfile.</returns>
        public static CopyProfile Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Profile file not found.", filePath);
            }

            string json = File.ReadAllText(filePath);
            var profile = JsonSerializer.Deserialize<CopyProfile>(json);
            
            if (profile == null)
            {
                throw new InvalidOperationException("Failed to deserialize profile.");
            }

            return profile;
        }

        /// <summary>
        /// Gets the default profiles directory.
        /// </summary>
        /// <returns>The path to the profiles directory.</returns>
        public static string GetProfilesDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string profilesDir = Path.Combine(appData, "RobocopyGUI", "Profiles");
            
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }
            
            return profilesDir;
        }

        /// <summary>
        /// Creates a copy of this profile with a new name.
        /// </summary>
        /// <param name="newName">The new profile name.</param>
        /// <returns>A new CopyProfile instance.</returns>
        public CopyProfile Clone(string newName)
        {
            return new CopyProfile
            {
                Name = newName,
                Description = Description,
                Options = Options.Clone(),
                CreatedDate = DateTime.Now,
                LastModifiedDate = DateTime.Now,
                LastUsedDate = null
            };
        }
    }
}
