﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Configurations
{
    internal class OrderSummaryConfiguration : IEntityTypeConfiguration<OrderSummary>
    {
        public void Configure(EntityTypeBuilder<OrderSummary> builder)
        {
            //builder.ToTable((string?)null); //nie tworzy tabeli dla encji, a jedynie uświadamia EF o istnieniu tego typu
            builder.ToView("View_OrderSummary");
        }
    }
}
