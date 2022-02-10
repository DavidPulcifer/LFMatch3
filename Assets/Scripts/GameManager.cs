using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoardGoal))]
public class GameManager : Singleton<GameManager>
{    
    Board m_board;

    bool m_isReadyToBegin = false;

    bool m_isGameOver = false;
    public bool IsGameOver { get => m_isGameOver; set => m_isGameOver = value; }

    bool m_isWinner = false;
    bool m_isReadyToReload = false;

    //LevelGoal m_levelGoal; - Level Goal
    BoardGoal m_boardGoal;
    public BoardGoal BoardGoal { get => m_boardGoal; }

    //LevelGoalTimed m_levelGoalTimed; - Level Goal

    //LevelGoalCollected m_levelGoalCollected; - Level Goal

    //public LevelGoal LevelGoal { get { return m_levelGoal; } } - Level Goal

    public override void Awake()
    {
        base.Awake();

        //m_levelGoal = GetComponent<LevelGoal>(); - Level Goal
        m_boardGoal = GetComponent<BoardGoal>();

        //m_levelGoalTimed = GetComponent<LevelGoalTimed>();
        //m_levelGoalCollected = GetComponent<LevelGoalCollected>(); - Level Goal

        m_board = FindObjectOfType<Board>().GetComponent<Board>();

        
    }
        
    void Start()
    {
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.scoreMeter != null)
            {
                //UIManager.Instance.scoreMeter.SetupStars(m_levelGoal); - Level Goal
                UIManager.Instance.scoreMeter.SetupStars(m_boardGoal);
            }

            if (UIManager.Instance.levelNameText != null)
            {
                Scene scene = SceneManager.GetActiveScene();
                UIManager.Instance.levelNameText.text = scene.name;
            }

            if (m_boardGoal.PiecesToCollect != null)
            {
                UIManager.Instance.EnableCollectionGoalLayout(true);
                UIManager.Instance.SetupCollectionGoalLayout(m_boardGoal.PiecesToCollect);
            }
            else
            {
                UIManager.Instance.EnableCollectionGoalLayout(false);
            }

            bool useTimer = (m_boardGoal.BoardCounter == BoardCounter.Timer);

            UIManager.Instance.EnableTimer(useTimer);
            UIManager.Instance.EnableMovesCounter(!useTimer);
        }
        
        m_boardGoal.MovesLeft++;
        UpdateMoves();

        StartCoroutine("ExecuteGameLoop");
    }

    public void UpdateMoves()
    {
        if(m_boardGoal.BoardCounter == BoardCounter.Moves)
        {
            m_boardGoal.MovesLeft--;

            if (UIManager.Instance != null && UIManager.Instance.movesLeftText != null)
            {
                UIManager.Instance.movesLeftText.text = m_boardGoal.MovesLeft.ToString();
            }            
        }        
    }

    IEnumerator ExecuteGameLoop()
    {
        yield return StartCoroutine("StartGameRoutine");
        yield return StartCoroutine("PlayGameRoutine");
        yield return StartCoroutine("WaitForBoardRoutine", 0.5f);
        yield return StartCoroutine("EndGameRoutine");
    }

    public void BeginGame()
    {
        m_isReadyToBegin = true;
    }

    IEnumerator StartGameRoutine()
    {
        if(UIManager.Instance != null)
        {
            if (UIManager.Instance.messageWindow != null)
            {
                UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
                int maxGoal = m_boardGoal.ScoreGoals.Length - 1;
                UIManager.Instance.messageWindow.ShowScoreMessage(m_boardGoal.ScoreGoals[maxGoal]);

                if(m_boardGoal.BoardCounter == BoardCounter.Timer)
                {
                    UIManager.Instance.messageWindow.ShowTimedGoal(m_boardGoal.TimeLeft);
                }
                else
                {
                    UIManager.Instance.messageWindow.ShowMovesGoal(m_boardGoal.MovesLeft);
                }

                if(m_boardGoal.PiecesToCollect != null)
                {
                    if (m_boardGoal.PiecesToCollect.Length == 0)
                    {
                        UIManager.Instance.messageWindow.ShowCollectionGoal(false);
                    }
                    else
                    {
                        UIManager.Instance.messageWindow.ShowCollectionGoal(true);
                    }                    

                    GameObject goalLayout = UIManager.Instance.messageWindow.collectionGoalLayout;

                    if(goalLayout != null)
                    {
                        UIManager.Instance.SetupCollectionGoalLayout(m_boardGoal.PiecesToCollect, goalLayout, 100);
                    }
                }                
            }
        }        

        while (!m_isReadyToBegin)
        {
            yield return null;            
        }

        if (UIManager.Instance != null && UIManager.Instance.screenFader != null)
        {
            UIManager.Instance.screenFader.FadeOff();
        }

        yield return new WaitForSeconds(0.5f);

        if(m_board != null)
        {
            m_board.SetupBoard();
        }
    }

    IEnumerator PlayGameRoutine()
    {
        if(m_boardGoal.BoardCounter == BoardCounter.Timer)
        {
            m_boardGoal.StartCountdown();
        }

        while (!m_isGameOver)
        {
            m_isGameOver = m_boardGoal.IsGameOver();
            m_isWinner = m_boardGoal.IsWinner();
            yield return null;
        }        
    }

    IEnumerator WaitForBoardRoutine(float delay = 0f)
    {
        if (m_boardGoal.BoardCounter == BoardCounter.Timer && 
            UIManager.Instance != null && UIManager.Instance.timer != null)
        {
            if(UIManager.Instance.timer != null)
            {
                UIManager.Instance.timer.FadeOff();
                UIManager.Instance.timer.paused = true;
            }
        }

        if (m_board != null)
        {
            yield return new WaitForSeconds(m_board.swapTime);
            while (m_board.isRefilling)
            {
                yield return null;
            }
        }
        yield return new WaitForSeconds(delay);
    }

    IEnumerator EndGameRoutine()
    {
        m_isReadyToReload = false;        

        if (m_isWinner)
        {
            ShowWinScreen();
        }
        else
        {
            ShowLoseScreen();

        }

        yield return new WaitForSeconds(1f);

        if (UIManager.Instance != null && UIManager.Instance.screenFader != null)
        {
            UIManager.Instance.screenFader.FadeOn();
        }

        while (!m_isReadyToReload) yield return null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);        
    }

    void ShowLoseScreen()
    {
        if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
        {
            UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowLoseMessage();
            UIManager.Instance.messageWindow.ShowCollectionGoal(false);

            string caption = "";
            if(m_boardGoal.BoardCounter == BoardCounter.Timer)
            {
                caption = "Out Of Time!";
            }
            else
            {
                caption = "Out Of Moves!";
            }

            UIManager.Instance.messageWindow.ShowGoalCaption(caption, 0, 0);

            if (UIManager.Instance.messageWindow.goalFailedIcon != null)
            {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalFailedIcon);
            }
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLoseSound();
        }
    }

    void ShowWinScreen()
    {
        if (UIManager.Instance != null && UIManager.Instance.messageWindow != null)
        {
            UIManager.Instance.messageWindow.ShowWinMessage();
            UIManager.Instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
            UIManager.Instance.messageWindow.ShowCollectionGoal(false);

            if(ScoreManager.Instance != null)
            {
                string scoreStr = "you scored\n" + ScoreManager.Instance.CurrentScore.ToString() + " points!";
                UIManager.Instance.messageWindow.ShowGoalCaption(scoreStr, 0, 0);
            }            

            if(UIManager.Instance.messageWindow.goalCompleteIcon != null)
            {
                UIManager.Instance.messageWindow.ShowGoalImage(UIManager.Instance.messageWindow.goalCompleteIcon);
            }
        }
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWinSound();
        }
    }

    public void ReloadScene()
    {
        m_isReadyToReload = true;
    }

    public void ScorePoints(GamePiece piece, int multiplier = 1, int bonus = 0)
    {
        if (piece == null) return;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(piece.scoreValue * multiplier + bonus);
            m_boardGoal.UpdateScoreStars(ScoreManager.Instance.CurrentScore);

            if(UIManager.Instance != null && UIManager.Instance.scoreMeter != null)
            {
                UIManager.Instance.scoreMeter.UpdateScoreMeter(ScoreManager.Instance.CurrentScore, m_boardGoal.ScoredStars);
            }
        }

        if (ScoreManager.Instance != null && piece.clearSound != null)
        {
            SoundManager.Instance.PlayClipAtPoint(piece.clearSound, Vector3.zero, SoundManager.Instance.fxVolume);
        }
    }

    public void AddTime(int timeValue)
    {
        if(m_boardGoal.BoardCounter == BoardCounter.Timer)
        {
            m_boardGoal.AddTime(timeValue);
        }
    }

    public void UpdateCollectionGoals(GamePiece pieceToCheck)
    {
        if (m_boardGoal.PiecesToCollect == null) return;
        m_boardGoal.UpdateGoals(pieceToCheck);
    }
}
