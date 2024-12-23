using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class PeerReviewRepository : BaseRepository<PeerReview>
    {
        public PeerReviewRepository(CipdbContext context) : base(context)
        {
        }
    }
}
