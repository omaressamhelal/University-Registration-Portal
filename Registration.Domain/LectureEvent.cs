using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registration.Domain
{
    public class LectureEvent
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public DateTime DateOfEvent { get; set; }
        public string ContentCovered { get; set; }
    }
}
