using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Crystals/CrystalDataList")]
public class CrystalDataList : ScriptableObject
{
    [SerializeField] private List<CrystalData> _crystals;

    public int CristalCount() => _crystals.Count;

    public CrystalData GetCrystal(int number) => _crystals.FirstOrDefault(x => x.Number == number);
}
