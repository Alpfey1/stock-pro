namespace ClassLibrary2
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json; // CHANGED

    public partial class warehouse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public warehouse()
        {
           
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        
    }
}