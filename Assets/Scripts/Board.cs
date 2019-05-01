using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    private Cristal[,] _cristals;
    private Vector2 _startPoint;

    private GameObject _cellContainer;
    private GameObject _cristalContainer;

    private bool _canSwap = true;
    private bool _isClick = false;

    private Vector2 _clickPointStart;
    private Vector2 _clickPointFinal;

    [SerializeField] private CrystalDataList _crystalList;
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameObject _cristalPrefab;
    [SerializeField] private GameObject _score;

    [SerializeField] private int _width;
    [SerializeField] private int _height;

    [SerializeField] private float _clickDistance;
    [SerializeField] private float _fallingDelay;

    

    private void OnValidate()
    {
        if (_width < 3)
            _width = 3;

        if (_width > 15)
            _width = 15;

        if (_height < 3)
            _height = 3;

        if (_height > 7)
            _height = 7;

        if (_clickDistance < 0)
            _clickDistance = 0;
    }

    private void Start()
    {
        _startPoint = transform.position - new Vector3((_width - 1) / 2f, (_height - 1) / 2f, 0f);
        _cristals = new Cristal[_width, _height];
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(_width, _height);

        _cellContainer = new GameObject("Cells");
        _cristalContainer = new GameObject("Cristals");
        _cellContainer.transform.SetParent(transform);
        _cristalContainer.transform.SetParent(transform);

        CreateCells();
        CreateCristals();
    }

    private void OnMouseDown()
    {
        if (!_canSwap)
            return;

        _clickPointStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _isClick = true;
    }

    private void OnMouseUp()
    {
        if (!_canSwap)
            return;

        if (!_isClick)
            return;

        _clickPointFinal = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _isClick = false;

        if (CheckFalseTouch())
            return;

        StartCoroutine(SwapTouchedCristals());
    }

    private void CreateCells()
    {
        GameObject cell;
        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
            {
                cell = Instantiate(_cellPrefab, new Vector2(_startPoint.x + i, _startPoint.y + j), Quaternion.identity);
                cell.name = "Cell [" + i.ToString() + ", " + j.ToString() + "]";
                cell.transform.SetParent(_cellContainer.transform);
            }
    }

    private void CreateCristals()
    {
        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
            {
                _cristals[i, j] = CreateCristal(i,j).GetComponent<Cristal>();
                _cristals[i, j].Create(GetValidateNumber(i, j));
            }
    }

    public void Restart()
    {
        if (!_canSwap)
            return;

        _isClick = false;

        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
                Destroy(_cristals[i, j].gameObject);

        _score.GetComponent<Score>().Clear();

        CreateCristals();
    }

    private GameObject CreateCristal(int x, int y)
    {
        GameObject cristal = Instantiate(_cristalPrefab, new Vector2(_startPoint.x + x, _startPoint.y + y), Quaternion.identity);
        cristal.name = "Cristal [" + x.ToString() + ", " + y.ToString() + "]";
        cristal.transform.SetParent(_cristalContainer.transform);

        return cristal;
    }

    private int GetValidateNumber(int x, int y)
    {
        List<int> validateCristalNumbers = new List<int>();

        for (int i = 1; i <= 5; i++) 
            validateCristalNumbers.Add(i);

        int bottomCristals = 0;
        int bottomCristalNumber = 0;
        if (y - 1 >= 0)
        {
            bottomCristals++;
            bottomCristalNumber = _cristals[x, y - 1].Number();

            if (y - 2 >= 0 && bottomCristalNumber == _cristals[x, y - 2].Number())
                bottomCristals++;
        }

        if (bottomCristals == 2)
            validateCristalNumbers.Remove(bottomCristalNumber);

        int leftCristals = 0;
        int leftCristalNumber = 0;
        if (x - 1 >= 0)
        {
            leftCristals++;
            leftCristalNumber = _cristals[x - 1, y].Number();

            if (x - 2 >= 0 && leftCristalNumber == _cristals[x - 2, y].Number())
                leftCristals++;
        }

        if (leftCristals == 2)
            validateCristalNumbers.Remove(leftCristalNumber);

        return validateCristalNumbers[Random.Range(0, validateCristalNumbers.Count)];
    }

    private bool CheckFalseTouch() => Mathf.Abs(_clickPointFinal.y - _clickPointStart.y) < _clickDistance && Mathf.Abs(_clickPointFinal.x - _clickPointStart.x) < _clickDistance;

    private IEnumerator SwapTouchedCristals()
    {
        int firstX = Mathf.FloorToInt(_clickPointStart.x - _startPoint.x + 0.5f);
        int firstY = Mathf.FloorToInt(_clickPointStart.y - _startPoint.y + 0.5f);

        if (firstX < 0 || firstX >= _width || firstY < 0 || firstY >= _height)
            yield break;

        int secondX = firstX;
        int secondY = firstY;

        float angle = Mathf.Atan2(_clickPointFinal.y - _clickPointStart.y, _clickPointFinal.x - _clickPointStart.x) * 180 / Mathf.PI;

        if (angle > -45 && angle <= 45)
            secondX++;
        else if (angle > 45 && angle <= 135)
            secondY++;
        else if (angle > -135 && angle <= -45)
            secondY--;
        else
            secondX--;

        if (secondX < 0 || secondX >= _width || secondY < 0 || secondY >= _height)
            yield break;

        Cristal first = _cristals[firstX, firstY];
        Cristal second = _cristals[secondX, secondY];

        if (first.Number() == second.Number())
            yield break;

        first.SwapMove(secondX - firstX, secondY - firstY);
        second.SwapMove(firstX - secondX, firstY - secondY);

        _canSwap = false;

        yield return new WaitUntil(() => !first.IsMove());

        SwapValue(ref _cristals[firstX, firstY], ref _cristals[secondX, secondY]);

        List<Cristal> matches = FindMatches();

        if (matches.Count == 0)
        {
            first.SwapMove(firstX - secondX, firstY - secondY);
            second.SwapMove(secondX - firstX, secondY - firstY);

            yield return new WaitUntil(() => !first.IsMove());

            SwapValue(ref _cristals[firstX, firstY], ref _cristals[secondX, secondY]);

            _canSwap = true;

            yield break;
        }

        StartCoroutine(RemoveMatches(matches));
    }

    private IEnumerator RemoveMatches(List<Cristal> matches)
    {
        int score = 0;
        for (int i = 0; i < matches.Count; i++)
        {
            score += (int)Mathf.Pow(2, i);
            matches[i].Remove();
        }

        _score.GetComponent<Score>().SetValue(score * 10);

        GetComponent<AudioSource>().pitch = 1 + Random.Range(-0.05f, 0.15f);
        GetComponent<AudioSource>().Play();
        
        yield return new WaitUntil(() => matches[0] == null);

        yield return new WaitForSeconds(_fallingDelay);

        StartCoroutine(CristlalMoveDown());
    }

    private IEnumerator CristlalMoveDown()
    {
        List<Cristal> cristalsForFalling = FindCristalsForMoveDown();

        while (cristalsForFalling.Count != 0)
        {
            for (int i = 0; i < cristalsForFalling.Count; i++)
                cristalsForFalling[i].MoveDown();

            yield return new WaitUntil(() => !cristalsForFalling[0].IsMove());

            cristalsForFalling = FindCristalsForMoveDown();
        }

        StartCoroutine(CreateNewCristals());
    }

    private IEnumerator CreateNewCristals()
    {
        List<Cristal> creatingCristals = new List<Cristal>();

        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
                if (_cristals[i, j] == null)
                {
                    _cristals[i, j] = CreateCristal(i, j).GetComponent<Cristal>();
                    _cristals[i, j].Create(Random.Range(0, _crystalList.CristalCount()) + 1);

                    creatingCristals.Add(_cristals[i, j]);
                }

        yield return new WaitUntil(() => creatingCristals[0].IsReady());            

        List<Cristal> matches = FindMatches();
        
        if (matches.Count == 0)
            _canSwap = true;
        else
            StartCoroutine(RemoveMatches(matches));
    }

    private List<Cristal> FindCristalsForMoveDown()
    {
        List<Cristal> FallingList = new List<Cristal>();

        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
                if (_cristals[i, j] == null)
                {
                    for (int k = j + 1; k < _height; k++)
                    {
                        if (_cristals[i, k] != null)
                            FallingList.Add(_cristals[i, k]);

                        _cristals[i, k - 1] = _cristals[i, k];
                    }                       

                    _cristals[i, _height - 1] = null;

                    break;
                }

        return FallingList;
    }

    private List<Cristal> FindMatches()
    {
        List<Cristal> MatchList = new List<Cristal>();
        for (int i = 0; i < _width; i++)
            for (int j = 0; j < _height; j++)
            {
                int cristalNumber = _cristals[i, j].Number();

                List<Cristal> top = new List<Cristal>();
                List<Cristal> bottom = new List<Cristal>();
                List<Cristal> right = new List<Cristal>();
                List<Cristal> left = new List<Cristal>();

                for (int k = 1; ; k++)
                {
                    if (i + k >= _width)
                        break;

                    if (_cristals[i + k, j].Number() != cristalNumber)
                        break;

                    top.Add(_cristals[i + k, j]);
                }
                for (int k = 1; ; k++)
                {
                    if (i - k < 0)
                        break;

                    if (_cristals[i - k, j].Number() != cristalNumber)
                        break;

                    bottom.Add(_cristals[i - k, j]);
                }
                for (int k = 1; ; k++)
                {
                    if (j + k >= _height)
                        break;

                    if (_cristals[i, j + k].Number() != cristalNumber)
                        break;

                    right.Add(_cristals[i, j + k]);
                }
                for (int k = 1; ; k++)
                {
                    if (j - k < 0)
                        break;

                    if (_cristals[i, j - k].Number() != cristalNumber)
                        break;

                    left.Add(_cristals[i, j - k]);
                }

                if (top.Count + bottom.Count > 1 && left.Count + right.Count > 1)
                {
                    MatchList.Add(_cristals[i, j]);
                    MatchList.AddRange(top);
                    MatchList.AddRange(bottom);
                    MatchList.AddRange(right);
                    MatchList.AddRange(left);
                }
                else if (top.Count + bottom.Count > 1)
                {
                    MatchList.Add(_cristals[i, j]);
                    MatchList.AddRange(top);
                    MatchList.AddRange(bottom);
                }
                else if (left.Count + right.Count > 1)
                {
                    MatchList.Add(_cristals[i, j]);
                    MatchList.AddRange(right);
                    MatchList.AddRange(left);
                }
            }

        return MatchList.Distinct().ToList();
    }

    private void SwapValue<T>(ref T first, ref T second)
    {
        T temp = first;
        first = second;
        second = temp;
    }
}