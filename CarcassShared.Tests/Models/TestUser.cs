using System;

namespace CarcassShared.Tests.Models
{
    public class TestUser : IEquatable<TestUser>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public bool Equals(TestUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FirstName == other.FirstName && LastName == other.LastName && UserName == other.UserName &&
                   Password == other.Password && Email == other.Email;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TestUser)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, LastName, UserName, Password, Email);
        }
    }
}