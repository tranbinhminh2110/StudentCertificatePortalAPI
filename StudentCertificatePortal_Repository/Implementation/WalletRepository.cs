﻿using StudentCertificatePortal_Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentCertificatePortal_Repository.Implementation
{
    public class WalletRepository: BaseRepository<Wallet>
    {
        public WalletRepository(CipdbContext context) : base(context) { }
    }
}
