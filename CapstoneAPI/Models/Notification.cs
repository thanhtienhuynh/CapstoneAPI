using System;
using System.Collections.Generic;

#nullable disable

namespace CapstoneAPI.Models
{
    public partial class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime DateRecord { get; set; }
        public bool IsRead { get; set; }
        public string Data { get; set; }
        public int UserId { get; set; }
        public int Type { get; set; }
        //1: New article
        //2: New rank
        //3: Update tt uni
        //5: Bai viet crawl moi
        //6: Thay doi tt suggest => remove follow
        
        public virtual User User { get; set; }
    }
}
