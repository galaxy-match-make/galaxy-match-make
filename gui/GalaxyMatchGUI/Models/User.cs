using System;
using System.Collections.Generic;

namespace GalaxyMatchGUI.Models
{
    public class User
    {
        public User()
        {
            SentMessages = new HashSet<Message>();
            ReceivedMessages = new HashSet<Message>();
            UserInterests = new HashSet<UserInterest>();
        }

        public Guid Id { get; set; }
        public required string OauthId { get; set; }
        public bool Inactive { get; set; }
        
        // Navigation properties
        public Profile? Profile { get; set; }  // Made optional by removing required keyword and adding nullable
        public ICollection<UserInterest> UserInterests { get; set; }
        // public ICollection<Contact> SentReactions { get; set; }
        // public ICollection<Contact> ReceivedReactions { get; set; }
        public ICollection<Message> SentMessages { get; set; }
        public ICollection<Message> ReceivedMessages { get; set; }
    }
}