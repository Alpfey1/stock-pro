namespace ClassLibrary2
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json; // CHANGED

    public partial class user
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public user()
        {
            
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Password_hash { get; set; }
        public Nullable<System.DateTime> Created_at { get; set; }

        
    }
}