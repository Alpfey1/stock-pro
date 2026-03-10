namespace ClassLibrary2
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using Newtonsoft.Json; // CHANGED

    public partial class product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public product()
        {

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public string Unit_type { get; set; }
        public string Category { get; set; }

        [Column("Supplier_id")]
        public Nullable<int> Supplier_id { get; set; }

       

        [ForeignKey("Supplier_id")]
        public virtual supplier supplier { get; set; }

      

    
    }
}