using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    private Text _text;
    private int _score = 0;

    private void Start()
    {
        _text = GetComponent<Text>();
    }

    public void Clear()
    {
        _score = 0;
        _text.text = _score.ToString();
    }

    public void SetValue(int value)
    {
        _score += value;
        _text.text = _score.ToString();
    }
}
