using System;
using System.Collections.Generic;

namespace QLTV.Models;

public partial class TUASACH
{
    public int ID { get; set; }

    public string? MaTuaSach { get; set; }

    public string TenTuaSach { get; set; } = null!;

    public string BiaSach { get; set; } = null!;

    public int SoLuong { get; set; }

    public int HanMuonToiDa { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<SACH> SACH { get; set; } = new List<SACH>();

    public virtual ICollection<TACGIA> IDTacGia { get; set; } = new List<TACGIA>();

    public virtual ICollection<THELOAI> IDTheLoai { get; set; } = new List<THELOAI>();
}
