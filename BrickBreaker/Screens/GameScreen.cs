﻿/*  Created by: 
 *  Project: Brick Breaker
 *  Date: 
 */ 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Threading;
using System.Xml;

namespace BrickBreaker
{
    public partial class GameScreen : UserControl
    {
        #region global values

        //player1 button control keys - DO NOT CHANGE
        Boolean leftArrowDown, rightArrowDown;

        // Game values
        int lives;

        // Paddle and Ball objects
        Paddle paddle;
        Ball ball;

        // list of all blocks for current level
        List<Block> blocks = new List<Block>();

        // Brushes
        SolidBrush paddleBrush = new SolidBrush(Color.White);
        SolidBrush ballBrush = new SolidBrush(Color.White);
        SolidBrush blockBrush = new SolidBrush(Color.Red);
        Pen lineBrush = new Pen(Color.Aquamarine);

        int levelNumber = 1;

        // Controls if objects are allowed to move
        bool ballMove = false;
        bool paddleMove = false;

        // Controls launch line
        int launchLine = 2;
        #endregion

        public GameScreen()
        {
            InitializeComponent();

            OnStart();
        }


        public void OnStart()
        {
            
            //set life counter
            lives = 3;

            //set all button presses to false.
            leftArrowDown = rightArrowDown  = false;

            // setup starting paddle values and create paddle object
            int paddleWidth = 80;
            int paddleHeight = 20;
            int paddleX = ((this.Width / 2) - (paddleWidth / 2));
            int paddleY = (this.Height - paddleHeight) - 60;
            int paddleSpeed = 8;
            paddle = new Paddle(paddleX, paddleY, paddleWidth, paddleHeight, paddleSpeed, Color.White);

            // setup starting ball values
            int ballX = this.Width / 2 - 10;
            int ballY = this.Height - paddle.height - 80;

            // Creates a new ball
            int xSpeed = 6;
            int ySpeed = 6;
            int ballSize = 20;
            ball = new Ball(ballX, ballY, xSpeed, ySpeed, ballSize);

            //#region Creates blocks for generic level. Need to replace with code that loads levels.

            ////TODO - replace all the code in this region eventually with code that loads levels from xml files

            //blocks.Clear();
            //int x = 10;

            //while (blocks.Count < 12)
            //{
            //    x += 57;
            //    Block b1 = new Block(x, 10, 1, Color.White);
            //    blocks.Add(b1);
            //}

            //#endregion

            LevelStart();
        }

        private void GameScreen_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //player 1 button presses
            switch (e.KeyCode)
            {
                // Makes launch line change when arrow buttons are pressed
                case Keys.Left:
                    leftArrowDown = true;
                    if (paddleMove == false && launchLine > 1)
                    {
                        launchLine--;
                    }
                    break;
                case Keys.Right:
                    rightArrowDown = true;
                    if (paddleMove == false && launchLine < 4)
                    {
                        launchLine++;
                    }
                    break;
                case Keys.Space:
                    if (ballMove == false)
                    {
                        switch (launchLine)
                        {
                            case 1:
                                ball.xSpeed = -9;
                                ball.ySpeed = 3;
                                break;
                            case 2:
                                ball.xSpeed = -6;
                                ball.ySpeed = 6;
                                break;
                            case 3:
                                ball.xSpeed = 6;
                                ball.ySpeed = 6;
                                break;
                            case 4:
                                ball.xSpeed = 9;
                                ball.ySpeed = 3;
                                break;
                            default:
                                break;
                        }
                    }
                    ballMove = true;
                    paddleMove = true;
                    //TODO: Make ball trajectory match launch line
                    break;
                default:
                    break;
            }
        }

        private void GameScreen_KeyUp(object sender, KeyEventArgs e)
        {
            //player 1 button releases
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = false;
                    break;
                case Keys.Right:
                    rightArrowDown = false;
                    break;

                default:
                    break;
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if(paddle.width > 80)
            {
                paddle.width--;
            }

            // Move the paddle
            if (paddleMove == true)
            {
                if (leftArrowDown && paddle.x > 0)
                {
                    paddle.Move("left");
                }
                if (rightArrowDown && paddle.x < (this.Width - paddle.width))
                {
                    paddle.Move("right");
                }
            }
            

            // Move ball or show ball launch direction
            if (ballMove == true)
            { 
            ball.Move();
            }

            // Check for collision with top and side walls
            ball.WallCollision(this);

            // Check for ball hitting bottom of screen
            if (ball.BottomCollision(this))
            {
                lives--;

                // Moves the ball back to origin
                ball.x = ((paddle.x - (ball.size / 2)) + (paddle.width / 2));
                ball.y = (this.Height - paddle.height) - 85;
                ballMove = false;
                paddleMove = false;


                if (lives == 0)
                {
                    gameTimer.Enabled = false;
                    OnEnd();
                }
            }

            // Check for collision of ball with paddle, (incl. paddle movement)
            ball.PaddleCollision(paddle, leftArrowDown, rightArrowDown);

            // Check if ball has collided with any blocks
            foreach (Block b in blocks)
            {
                if (ball.BlockCollision(b))
                {
                    blocks.Remove(b);

                    if (blocks.Count == 0)
                    {
                        gameTimer.Enabled = false;
                        levelNumber++;
                        LevelStart();
                    }

                    break;
                }
            }

            //redraw the screen
            Refresh();
        }

        public void LevelStart()
        {
            //resets ball position
            ball.x = paddle.x - (ball.size / 2) + (paddle.width / 2);
            ball.y = Height - paddle.height - 85;
            ballMove = false;
            paddleMove = false;

            //gets level info and places blocks

            string level = "level" + levelNumber + ".xml";

            XmlTextReader reader = new XmlTextReader(level);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    int x = Convert.ToInt32(reader.ReadString());

                    reader.ReadToNextSibling("y");
                    int y = Convert.ToInt32(reader.ReadString());

                    reader.ReadToNextSibling("hp");
                    int hp = Convert.ToInt32(reader.ReadString());

                    reader.ReadToNextSibling("color");
                    string colour = reader.ReadString();

                    Color bColour = Color.White;

                    switch (colour)
                    {
                        case "DarkRed":
                            bColour = Color.DarkRed;
                            break;
                        case "Goldenrod":
                            bColour = Color.Goldenrod;
                            break;
                        case "YellowGreen":
                            bColour = Color.YellowGreen;
                            break;
                        case "DarkGray":
                            bColour = Color.DarkGray;
                            break;
                        default:
                            break;
                    }


                    Block block = new Block(x, y, hp, bColour);
                    blocks.Add(block);
                }
                // start the game engine loop
                gameTimer.Enabled = true;
            }
        }

        public void OnEnd()
        {
            // Goes to the game over screen
            Form form = this.FindForm();
            MenuScreen ps = new MenuScreen();
            
            ps.Location = new Point((form.Width - ps.Width) / 2, (form.Height - ps.Height) / 2);

            form.Controls.Add(ps);
            form.Controls.Remove(this);
        }

        public void GameScreen_Paint(object sender, PaintEventArgs e)
        {
            // Draws paddle
            paddleBrush.Color = paddle.colour;
            e.Graphics.FillRectangle(paddleBrush, paddle.x, paddle.y, paddle.width, paddle.height);

            // Draws blocks
            foreach (Block b in blocks)
            {
                e.Graphics.FillRectangle(blockBrush, b.x, b.y, b.width, b.height);
            }

            // Draws ball
            e.Graphics.FillRectangle(ballBrush, ball.x, ball.y, ball.size, ball.size);

            // Draws ball launch line if necessary
            if (ballMove == false)
            {
                // Makes launch line change depending on launchLine value
                switch (launchLine)
                {
                    case 1:
                        e.Graphics.DrawLine(lineBrush, paddle.x + (paddle.width / 2), paddle.y,
                            paddle.x + (paddle.width / 2) - 98, paddle.y - 51);
                        break;
                    case 2:
                        e.Graphics.DrawLine(lineBrush, paddle.x + (paddle.width / 2), paddle.y,
                            paddle.x + (paddle.width / 2) - 58, paddle.y - 69);
                        break;
                    case 3:
                        e.Graphics.DrawLine(lineBrush, paddle.x + (paddle.width / 2), paddle.y,
                            paddle.x + (paddle.width / 2) + 58, paddle.y - 69);
                        break;
                    case 4:
                        e.Graphics.DrawLine(lineBrush, paddle.x + (paddle.width / 2), paddle.y,
                            paddle.x + (paddle.width / 2) + 98, paddle.y - 51);
                        break;
                    default:
                        break;
                }

            }
        }

    }
}
    

