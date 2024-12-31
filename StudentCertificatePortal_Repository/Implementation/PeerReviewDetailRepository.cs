using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class PeerReviewDetailRepository : BaseRepository<PeerReviewDetail>

    {
        public PeerReviewDetailRepository(CipdbContext context) : base(context) { }
    }
}
