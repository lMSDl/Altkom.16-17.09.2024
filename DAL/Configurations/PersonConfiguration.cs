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
    internal class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Persons", x =>
            x.IsTemporal(xx =>
            {
                xx.HasPeriodStart("From"); //domyślnie: PeriodStart
                xx.HasPeriodEnd("To"); //domyślnie PeriodEnd
            }));

            builder.Property(x => x.Description).IsSparse();

            builder.OwnsOne(x => x.Address, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
                ownedNavigationBuilder.OwnsOne(x => x.Coordinates);
            });
        }
    }
}
