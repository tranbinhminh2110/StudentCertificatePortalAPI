using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class UserAnswerRepository: BaseRepository<UserAnswer>
    {
        public UserAnswerRepository(CipdbContext context) : base(context) { }
    }
}
