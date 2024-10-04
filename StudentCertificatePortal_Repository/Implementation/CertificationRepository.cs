using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class CertificationRepository : BaseRepository<Certification>
    {
        public CertificationRepository(CipdbContext context) : base(context)
        {
        }
    }
}
