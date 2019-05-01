using System.Collections;
using UnityEngine;

public class Cristal : MonoBehaviour
{
    [SerializeField] private CrystalDataList _crystalList;
    [SerializeField] private float _resizingStep;
    [SerializeField] private float _swapStep;
    [SerializeField] private float _moveDownStep;

    private Transform _transform;
    private int _number;
    private bool _isReady = false;
    private bool _isMove = false;

    public bool IsReady() => _isReady;
    public bool IsMove() => _isMove;
    public int Number() => _number;

    private void OnValidate()
    {
        if (_resizingStep < 0)
            _resizingStep = 0;

        if (_swapStep < 0)
            _swapStep = 0;

        if (_moveDownStep < 0)
            _moveDownStep = 0;
    }

    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }

    public void Create(int number)
    {
        _number = number;
        gameObject.GetComponent<SpriteRenderer>().sprite = _crystalList.GetCrystal(_number).Sprite;
        StartCoroutine(CreateAnimation());
    }

    private IEnumerator CreateAnimation()
    {
        _transform.localScale = new Vector3(0f, 0f, 0f);
        while (_transform.localScale.x < 1)
        {
            _transform.localScale += new Vector3(_resizingStep, _resizingStep, _resizingStep);
            yield return null;
        }
        _transform.localScale = new Vector3(1f, 1f, 1f);

        _isReady = true;
    }

    public void SwapMove(int directionX, int directionY)
    {
        StartCoroutine(LerpMove(directionX, directionY));
    }

    private IEnumerator LerpMove(int directionX, int directionY)
    {
        _isMove = true;

        Vector2 finalPoint = _transform.position + new Vector3(directionX, directionY, 0);
        float step = _swapStep / 100f;
        while (Mathf.Pow(finalPoint.x - _transform.position.x, 2) + Mathf.Pow(finalPoint.y - _transform.position.y, 2) > step)
        {
            _transform.position = Vector2.Lerp(_transform.position, finalPoint, _swapStep);
            yield return null;
        }
        _transform.position = finalPoint;

        _isMove = false;
    }

    public void MoveDown()
    {
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        _isMove = true;

        Vector2 finalPoint = _transform.position + new Vector3(0, -1, 0);
        while (Mathf.Abs(finalPoint.y - _transform.position.y) > 0)
        {
            _transform.position = Vector2.MoveTowards(_transform.position, finalPoint, _moveDownStep);
            yield return null;
        }
        _transform.position = finalPoint;

        _isMove = false;
    }

    public void Remove()
    {
        StartCoroutine(RemoveAnimation());
    }

    private IEnumerator RemoveAnimation()
    {
        Instantiate(_crystalList.GetCrystal(_number).Effect, _transform.position, Quaternion.identity);

        _transform.localScale = new Vector3(1f, 1f, 1f);
        while (transform.localScale.x > 0)
        {
            _transform.localScale -= new Vector3(_resizingStep, _resizingStep, _resizingStep);
            yield return null;
        }
        _transform.localScale = new Vector3(0f, 0f, 0f);

        Destroy(gameObject);
    }
}
