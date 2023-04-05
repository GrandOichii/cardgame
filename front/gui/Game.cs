using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Shared;

namespace gui;

class CardTexture : Texture2D
{
    public CardTexture(GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
    {
    }

    public void Draw(SpriteBatch sb, int x, int y, CardState state) {
        sb.Draw(this, new Vector2(x, y) , Bounds, Color.White);
    }

}

class CardTextureCreator {
    public CardTextureCreator() {

    }

    public CardTexture Create(GraphicsDevice gDevice, int width, int height) {
        var result = new CardTexture(gDevice, width, height);
        Color[] data = new Color[height*width];

        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                data[j + i * width] = Color.White;

        for (int i = 0; i < width; i++) {
            data[i] = Color.Black;
            data[i + (height-1)*width] = Color.Black;
        }

        for (int i = 0; i < height; i++) {
            data[i*width] = Color.Black;
            data[i*width + width-1] = Color.Black;
        }

        result.SetData(data);
        return result;
    }
}

public class CGame : Game
{
    private static int HAND_CARD_TOP_OFFSET = 30;
    private static int BETWEEN_CARDS_IN_HAND = 40;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;


    private CardTexture _cardTex;
    private CardTexture _bigCardTex;


    private MatchState _lastState = MatchState.From(File.ReadAllText("../state_test.json"));

    public int WHeight { get; private set; } = 600;
    public int WWidth { get; private set; } = 800;

    public CGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = WWidth;
        _graphics.PreferredBackBufferHeight = WHeight;

        // Window.ClientSizeChanged += EventArg
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        var ctc = new CardTextureCreator();
        _cardTex = ctc.Create(_graphics.GraphicsDevice, 100, 130);
        _bigCardTex = ctc.Create(_graphics.GraphicsDevice, 150, 195);
        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();
        // _cardTex.Draw(_spriteBatch, 1, 1, _lastState.Players[0].Bond);
        DrawMyHand();
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void DrawMyHand() {
        var hand = _lastState.My.Hand;
        var x = BETWEEN_CARDS_IN_HAND + BETWEEN_CARDS_IN_HAND * hand.Length; // TODO change
        var y = WHeight - HAND_CARD_TOP_OFFSET;
        for (int i = 0; i < hand.Length; i++) {
            _cardTex.Draw(_spriteBatch, x, y, hand[i]);
            x -= BETWEEN_CARDS_IN_HAND;        
        }

    }



}
