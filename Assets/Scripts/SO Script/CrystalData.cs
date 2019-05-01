using UnityEngine;

[CreateAssetMenu(menuName = "Crystals/CrystalData")]
public class CrystalData : ScriptableObject
{
    [SerializeField] private int _number;
    [SerializeField] private Sprite _sprite;
    [SerializeField] private GameObject _effect;

    public void OnValidate()
    {
        if (_number < 0)
            _number = 0;
    }

    public int Number => _number;

    public Sprite Sprite => _sprite;

    public GameObject Effect => _effect;
}
