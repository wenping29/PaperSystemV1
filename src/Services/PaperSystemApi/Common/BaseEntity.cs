using System;
using System.ComponentModel.DataAnnotations;

namespace PaperSystemApi.Common
{
    public abstract class BaseEntity
    {
        [Key]
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;
    }

    public abstract class BaseEntity<T> : BaseEntity where T : struct
    {
        [Key]
        public new T Id { get; set; }
    }
}