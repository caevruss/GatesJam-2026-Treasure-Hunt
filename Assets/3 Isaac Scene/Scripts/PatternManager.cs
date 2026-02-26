using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatternArrowManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int columns = 4;
    [SerializeField] private List<PatternDot> dots;
    [SerializeField] private int startDotId = 0;

    [Header("Isaac (UI)")]
    [SerializeField] private RectTransform isaac;

    [Header("Movement Settings")]
    [Range(0.05f, 1.5f)]
    [SerializeField] private float moveDuration = 0.25f;

    [Header("Arrow placement")]
    [Range(20f, 120f)]
    [SerializeField] private float arrowRadius = 40f;

    [Header("Arrows (No rotate, just on/off)")]
    [SerializeField] private Button upArrow;
    [SerializeField] private Button downArrow;
    [SerializeField] private Button leftArrow;
    [SerializeField] private Button rightArrow;

    [Header("Diagonal Arrows")]
    [SerializeField] private Button upLeftArrow;
    [SerializeField] private Button upRightArrow;
    [SerializeField] private Button downLeftArrow;
    [SerializeField] private Button downRightArrow;

    [Header("Animation")]
    [SerializeField] private Animator isaacAnimator;
    [SerializeField] private string movingParam = "Moving"; // bool
    [SerializeField] private string dirXParam = "DirX";     // float
    [SerializeField] private string dirYParam = "DirY";     // float

    [Header("Lines (UI)")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private Transform lineParent;

    [Header("Solution")]
    [SerializeField] private List<int> correctSequence;

    [Header("Restart (when stuck)")]
    [SerializeField] private Button restartButton;

    [Header("Next Level")]
    [SerializeField] private NextLevelButton nextLevelButton;

    private readonly Dictionary<int, PatternDot> _dotById = new();
    private readonly List<int> _currentSequence = new();
    private readonly List<GameObject> _lines = new();

    private int _currentDotId;
    private bool _moving;
    private Coroutine _moveRoutine;

    void Awake()
    {
        // Auto-get animator if not assigned
        if (!isaacAnimator && isaac) isaacAnimator = isaac.GetComponent<Animator>();

        _dotById.Clear();
        foreach (var d in dots)
        {
            if (d == null) continue;
            _dotById[d.id] = d;
        }

        if (upArrow) upArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(0, 1)));
        if (downArrow) downArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(0, -1)));
        if (leftArrow) leftArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(-1, 0)));
        if (rightArrow) rightArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(1, 0)));

        if (upLeftArrow) upLeftArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(-1, 1)));
        if (upRightArrow) upRightArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(1, 1)));
        if (downLeftArrow) downLeftArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(-1, -1)));
        if (downRightArrow) downRightArrow.onClick.AddListener(() => OnArrowPressed(new Vector2Int(1, -1)));

        if (restartButton)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(() => ResetPattern()); // scene reload yerine reset istersen böyle
        }
    }

    void Start()
    {
        StartCoroutine(InitAfterLayout());
    }

    IEnumerator InitAfterLayout()
    {
        yield return null;
        yield return null;
        ResetPattern();
    }

    Vector2Int To4Dir(Vector2Int dir)
    {
        if (dir.x != 0 && dir.y != 0)
            return new Vector2Int(0, dir.y); // diagonal -> up/down
        return dir;
    }

    public void ResetPattern()
    {
        _currentSequence.Clear();
        foreach (var l in _lines) if (l) Destroy(l);
        _lines.Clear();

        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        _moving = false;

        if (isaacAnimator)
        {
            isaacAnimator.SetBool(movingParam, false);
            isaacAnimator.SetFloat(dirXParam, 0);
            isaacAnimator.SetFloat(dirYParam, 0);
        }

        if (restartButton) restartButton.gameObject.SetActive(false);
        if (nextLevelButton) nextLevelButton.SetUnlocked(false);

        _currentDotId = startDotId;
        _currentSequence.Add(_currentDotId);

        if (isaac && _dotById.TryGetValue(_currentDotId, out var startDot))
            isaac.position = startDot.transform.position;

        RefreshArrows();
    }

    void OnArrowPressed(Vector2Int dir)
    {
        if (_moving) return;

        int nextId = GetNeighborId(_currentDotId, dir);
        if (nextId < 0) return;
        if (_currentSequence.Contains(nextId)) return;

        DrawLine(_currentDotId, nextId);

        _currentDotId = nextId;
        _currentSequence.Add(_currentDotId);

        var d4 = To4Dir(dir);

        if (isaacAnimator)
        {
            isaacAnimator.SetBool(movingParam, true);
            isaacAnimator.SetFloat(dirXParam, d4.x);
            isaacAnimator.SetFloat(dirYParam, d4.y);
        }

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveIsaacTo(_dotById[_currentDotId].transform.position));

        if (_currentSequence.Count == correctSequence.Count)
            CheckResult();
        else
            RefreshArrows();
    }

    IEnumerator MoveIsaacTo(Vector3 worldTarget)
    {
        _moving = true;
        RefreshArrows();

        Vector3 start = isaac.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, moveDuration);
            isaac.position = Vector3.Lerp(start, worldTarget, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        isaac.position = worldTarget;
        _moving = false;

        if (isaacAnimator)
            isaacAnimator.SetBool(movingParam, false);

        RefreshArrows();
        _moveRoutine = null;
    }

    void RefreshArrows()
    {
        if (!isaac) return;

        PlaceArrow(upArrow, new Vector2Int(0, 1));
        PlaceArrow(downArrow, new Vector2Int(0, -1));
        PlaceArrow(leftArrow, new Vector2Int(-1, 0));
        PlaceArrow(rightArrow, new Vector2Int(1, 0));

        PlaceArrow(upLeftArrow, new Vector2Int(-1, 1));
        PlaceArrow(upRightArrow, new Vector2Int(1, 1));
        PlaceArrow(downLeftArrow, new Vector2Int(-1, -1));
        PlaceArrow(downRightArrow, new Vector2Int(1, -1));

        SetArrowActive(upArrow, new Vector2Int(0, 1));
        SetArrowActive(downArrow, new Vector2Int(0, -1));
        SetArrowActive(leftArrow, new Vector2Int(-1, 0));
        SetArrowActive(rightArrow, new Vector2Int(1, 0));

        SetArrowActive(upLeftArrow, new Vector2Int(-1, 1));
        SetArrowActive(upRightArrow, new Vector2Int(1, 1));
        SetArrowActive(downLeftArrow, new Vector2Int(-1, -1));
        SetArrowActive(downRightArrow, new Vector2Int(1, -1));

        UpdateRestartVisibility();
    }

    void PlaceArrow(Button btn, Vector2Int dir)
    {
        if (!btn) return;
        var rt = btn.GetComponent<RectTransform>();
        if (!rt) return;

        float radius = arrowRadius;
        if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            radius *= 0.9f;

        Vector2 offset = ((Vector2)dir).normalized * radius;
        rt.position = isaac.position + (Vector3)offset;
    }

    void SetArrowActive(Button btn, Vector2Int dir)
    {
        if (!btn) return;

        int nextId = GetNeighborId(_currentDotId, dir);

        bool valid =
            !_moving &&
            nextId >= 0 &&
            !_currentSequence.Contains(nextId);

        btn.gameObject.SetActive(valid);
        btn.interactable = valid;
    }

    bool HasValidMove(Vector2Int dir)
    {
        int nextId = GetNeighborId(_currentDotId, dir);
        if (nextId < 0) return false;
        if (_currentSequence.Contains(nextId)) return false;
        return true;
    }

    void UpdateRestartVisibility()
    {
        if (!restartButton) return;

        if (_moving)
        {
            restartButton.gameObject.SetActive(false);
            return;
        }

        bool hasMove =
            HasValidMove(new Vector2Int(0, 1)) ||
            HasValidMove(new Vector2Int(0, -1)) ||
            HasValidMove(new Vector2Int(-1, 0)) ||
            HasValidMove(new Vector2Int(1, 0)) ||
            HasValidMove(new Vector2Int(-1, 1)) ||
            HasValidMove(new Vector2Int(1, 1)) ||
            HasValidMove(new Vector2Int(-1, -1)) ||
            HasValidMove(new Vector2Int(1, -1));

        restartButton.gameObject.SetActive(!hasMove);
        restartButton.interactable = !hasMove;
    }

    // UI'da aşağı = y azalır => ny = y - dir.y
    int GetNeighborId(int fromId, Vector2Int dir)
    {
        int x = fromId % columns;
        int y = fromId / columns;

        int nx = x + dir.x;
        int ny = y - dir.y;

        if (nx < 0 || nx >= columns) return -1;
        if (ny < 0) return -1;

        int nextId = ny * columns + nx;
        return _dotById.ContainsKey(nextId) ? nextId : -1;
    }

    void CheckResult()
    {
        if (_currentSequence.Count != correctSequence.Count)
        {
            ResetPattern();
            return;
        }

        for (int i = 0; i < correctSequence.Count; i++)
        {
            if (_currentSequence[i] != correctSequence[i])
            {
                ResetPattern();
                return;
            }
        }

        Debug.Log("TEBRİKLER! Kilit Açıldı.");

        HideAllArrows();

        if (restartButton) restartButton.gameObject.SetActive(false);
        if (nextLevelButton) nextLevelButton.SetUnlocked(true);
    }

    void HideAllArrows()
    {
        if (upArrow) upArrow.gameObject.SetActive(false);
        if (downArrow) downArrow.gameObject.SetActive(false);
        if (leftArrow) leftArrow.gameObject.SetActive(false);
        if (rightArrow) rightArrow.gameObject.SetActive(false);

        if (upLeftArrow) upLeftArrow.gameObject.SetActive(false);
        if (upRightArrow) upRightArrow.gameObject.SetActive(false);
        if (downLeftArrow) downLeftArrow.gameObject.SetActive(false);
        if (downRightArrow) downRightArrow.gameObject.SetActive(false);
    }

    void DrawLine(int fromId, int toId)
    {
        if (!_dotById.TryGetValue(fromId, out var a)) return;
        if (!_dotById.TryGetValue(toId, out var b)) return;

        var go = Instantiate(linePrefab, lineParent);
        _lines.Add(go);

        var rect = go.GetComponent<RectTransform>();
        UpdateLineSize(rect, a.transform.position, b.transform.position);
    }

    void UpdateLineSize(RectTransform lineRect, Vector3 startPos, Vector3 endPos)
    {
        Vector3 dir = endPos - startPos;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRect.position = startPos;
        lineRect.pivot = new Vector2(0, 0.5f);

        lineRect.sizeDelta = new Vector2(distance / lineParent.lossyScale.x, 20f);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }
}