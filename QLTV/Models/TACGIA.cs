﻿using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class TACGIA
{
    public int ID { get; set; }

    public string? MaTacGia { get; set; }

    public int NamSinh { get; set; }

    public string QuocTich { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<TUASACH> IDTuaSach { get; set; } = new List<TUASACH>();
}
