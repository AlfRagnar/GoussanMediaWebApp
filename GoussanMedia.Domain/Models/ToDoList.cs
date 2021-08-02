using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace GoussanMedia.Domain.Models
{
    public class ToDoList
    {
        // Set by User Interaction
        [Required]
        [StringLength(16, ErrorMessage = "Document title too long (16 character limit)")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [Required]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [Required]
        [JsonProperty(PropertyName = "isComplete")]
        public bool Completed { get; set; }

        // Set by the Application

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }

        [JsonProperty(PropertyName = "changed")]
        public DateTime Changed { get; set; }
    }
}