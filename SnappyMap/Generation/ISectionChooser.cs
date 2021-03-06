﻿namespace SnappyMap.Generation
{
    using System;
    using System.Collections.Generic;

    using SnappyMap.Data;
    using SnappyMap.Database;

    public interface ISectionChooser
    {
        Section ChooseSectionOfType(SectionType type);
    }

    public class SectionDatabase : ISectionChooser, ISectionDatabase
    {
        private readonly Dictionary<SectionType, List<Section>> store = new Dictionary<SectionType, List<Section>>();

        private readonly Random randomSource = new Random();

        public void RegisterSection(Section section, SectionType type)
        {
            if (!this.store.ContainsKey(type))
            {
                this.store[type] = new List<Section>();
            }

            this.store[type].Add(section);
        }

        public Section ChooseSectionOfType(SectionType type)
        {
            var list = this.store[type];
            int choice = this.randomSource.Next(list.Count);
            return list[choice];
        }
    }
}