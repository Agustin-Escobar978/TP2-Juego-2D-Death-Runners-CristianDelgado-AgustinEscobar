using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Death_Runners
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private Texture2D mapa;
        private Texture2D motoAzul;
        private Texture2D motoRoja;
        private Texture2D motoAzulSalto;
        private Texture2D motoRojaSalto;
        private Texture2D menu;
        private Texture2D linea;
        private Texture2D trampasImagen;

        private bool menuActivo = true;
        private MouseState mouseAnterior;
        private KeyboardState tecladoAnterior;

        private Random aleatorio = new Random();

        private int motoAzulX = 40;
        private int motoRojaX = 40;

        // Alturas de los tres carriles del mapa original.
        private int[] caminos = { 384, 411, 441 };
        private int carrilAzul;
        private int carrilRojo;

        private int anchoMoto = 60;
        private int altoMoto = 40;
        private int velocidad = 3;

        private int anchoPantalla = 1280;
        private int altoPantalla = 720;

        private Rectangle recorteNormal = new Rectangle(379, 15, 193, 128);

        private Rectangle[] recortesAceleracion =
        {
            new Rectangle(29, 168, 193, 128),
            new Rectangle(264, 168, 193, 128),
            new Rectangle(500, 168, 218, 128),
            new Rectangle(765, 168, 238, 128)
        };

        private Rectangle[] recortesFrenado =
        {
            new Rectangle(4, 315, 218, 128),
            new Rectangle(310, 315, 265, 128),
            new Rectangle(655, 315, 234, 128)
        };

        private Rectangle[] recortesSalto =
        {
            new Rectangle(4, 45, 516, 456),
            new Rectangle(537, 30, 465, 326),
            new Rectangle(29, 599, 472, 305),
            new Rectangle(547, 603, 467, 327)
        };

        private bool azulAcelerando;
        private bool azulFrenando;
        private bool azulVeniaAvanzando;
        private int frameAzul;
        private int frameFrenadoAzul;
        private float tiempoAzul;

        private bool rojaAcelerando;
        private bool rojaFrenando;
        private bool rojaVeniaAvanzando;
        private int frameRojo;
        private int frameFrenadoRojo;
        private float tiempoRojo;

        private bool azulSaltando;
        private int frameSaltoAzul;
        private float tiempoSaltoAzul;

        private bool rojaSaltando;
        private int frameSaltoRojo;
        private float tiempoSaltoRojo;

        private float tiempoPorFrameSalto = 0.13f;

        // Recortes del archivo trampas.png (1024 x 683).
        // Cada rectángulo encierra una imagen individual del spritesheet.
        private Rectangle recortePinchos = new Rectangle(319, 202, 82, 54);
        private Rectangle recorteSierra = new Rectangle(615, 155, 155, 102);
        private Rectangle recorteBarril = new Rectangle(38, 382, 82, 113);

        private class Trampa
        {
            public int X;
            public int Carril;
            public int Tipo; // 0 = pinchos, 1 = sierra, 2 = barril

            public Trampa(int x, int carril, int tipo)
            {
                X = x;
                Carril = carril;
                Tipo = tipo;
            }
        }

        private List<Trampa> trampas = new List<Trampa>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            graphics.PreferredBackBufferWidth = anchoPantalla;
            graphics.PreferredBackBufferHeight = altoPantalla;
            graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            menu = Content.Load<Texture2D>("Juego_Moto_Img/menu");
            mapa = Content.Load<Texture2D>("Juego_Moto_Img/mapa");
            motoAzul = Content.Load<Texture2D>("Juego_Moto_Img/moto_azul");
            motoRoja = Content.Load<Texture2D>("Juego_Moto_Img/moto_rojo");
            motoAzulSalto = Content.Load<Texture2D>("Juego_Moto_Img/moto_azul_salto");
            motoRojaSalto = Content.Load<Texture2D>("Juego_Moto_Img/moto_rojo_salto");

            // El archivo debe llamarse trampas.png o trampa.png dentro de
            // Content/Juego_Moto_Img. Si en tu proyecto se llama trampa.png,
            // esta ruta está escrita correctamente como "trampa".
            trampasImagen = Content.Load<Texture2D>("Juego_Moto_Img/trampas");

            linea = new Texture2D(GraphicsDevice, 1, 1);
            linea.SetData(new[] { Color.White });

            carrilAzul = aleatorio.Next(0, 3);
            carrilRojo = aleatorio.Next(0, 2);
            if (carrilRojo >= carrilAzul)
                carrilRojo++;

            CrearTrampas();
        }

        private void CrearTrampas()
        {
            // Posiciones distribuidas a lo largo del recorrido.
            // Los carriles alternan para que haya obstáculos en los tres.
            trampas.Clear();

            trampas.Add(new Trampa(310, 0, 0));
            trampas.Add(new Trampa(405, 1, 1));
            trampas.Add(new Trampa(505, 2, 2));
            trampas.Add(new Trampa(625, 1, 0));
            trampas.Add(new Trampa(755, 0, 1));
            trampas.Add(new Trampa(755, 2, 0));
            trampas.Add(new Trampa(875, 2, 2));
            trampas.Add(new Trampa(1000, 1, 0));
            trampas.Add(new Trampa(1130, 0, 1));
            trampas.Add(new Trampa(1260, 2, 2));
            trampas.Add(new Trampa(1390, 1, 0));
            trampas.Add(new Trampa(1520, 0, 1));
            trampas.Add(new Trampa(1650, 2, 2));
            trampas.Add(new Trampa(1780, 1, 0));
            trampas.Add(new Trampa(1900, 0, 1));
        }

        private Rectangle AjustarRecorte(Texture2D textura, Rectangle recorte)
        {
            int x = MathHelper.Clamp(recorte.X, 0, textura.Width - 1);
            int y = MathHelper.Clamp(recorte.Y, 0, textura.Height - 1);
            int ancho = Math.Min(recorte.Width, textura.Width - x);
            int alto = Math.Min(recorte.Height, textura.Height - y);
            return new Rectangle(x, y, ancho, alto);
        }

        private Rectangle ObtenerRecorte(bool acelerando, bool frenando,
            int frameAceleracion, int frameFrenado)
        {
            if (acelerando)
                return recortesAceleracion[frameAceleracion];
            if (frenando)
                return recortesFrenado[frameFrenado];
            return recorteNormal;
        }

        private void ActualizarAnimacionMoto(bool avanzaDerecha, bool seMueve,
            ref bool veniaAvanzando, ref bool acelerando, ref bool frenando,
            ref int frameAceleracion, ref int frameFrenado, ref float tiempo,
            float delta)
        {
            if (avanzaDerecha)
            {
                if (!acelerando)
                {
                    frameAceleracion = 0;
                    tiempo = 0;
                }

                acelerando = true;
                frenando = false;
                veniaAvanzando = true;
                tiempo += delta;

                if (frameAceleracion < recortesAceleracion.Length - 1 &&
                    tiempo >= 0.12f)
                {
                    frameAceleracion++;
                    tiempo = 0;
                }
                return;
            }

            if (seMueve)
            {
                acelerando = false;
                frenando = false;
                veniaAvanzando = false;
                frameAceleracion = 0;
                frameFrenado = 0;
                tiempo = 0;
                return;
            }

            if (veniaAvanzando)
            {
                acelerando = false;
                frenando = true;
                veniaAvanzando = false;
                frameFrenado = 0;
                tiempo = 0;
                return;
            }

            if (frenando)
            {
                tiempo += delta;
                if (tiempo >= 0.12f)
                {
                    frameFrenado++;
                    tiempo = 0;
                    if (frameFrenado >= recortesFrenado.Length)
                    {
                        frenando = false;
                        frameFrenado = 0;
                        frameAceleracion = 0;
                    }
                }
            }
        }

        private void ActualizarSalto(bool teclaSalto, ref bool saltando,
            ref int frame, ref float tiempo, float delta)
        {
            if (teclaSalto && !saltando)
            {
                saltando = true;
                frame = 0;
                tiempo = 0;
            }

            if (saltando)
            {
                tiempo += delta;
                if (tiempo >= tiempoPorFrameSalto)
                {
                    tiempo = 0;
                    frame++;
                    if (frame >= recortesSalto.Length)
                    {
                        saltando = false;
                        frame = 0;
                        tiempo = 0;
                    }
                }
            }
        }

        private float ObtenerAlturaSalto(bool saltando, int frame)
        {
            if (!saltando)
                return 0;
            float[] alturas = { 12, 34, 34, 12 };
            return alturas[frame];
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState teclado = Keyboard.GetState();
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (teclado.IsKeyDown(Keys.Escape))
                Exit();

            if (menuActivo)
            {
                MouseState mouse = Mouse.GetState();

                if (mouse.LeftButton == ButtonState.Pressed &&
                    mouseAnterior.LeftButton == ButtonState.Released)
                {
                    if (new Rectangle(450, 350, 380, 85).Contains(mouse.X, mouse.Y))
                        menuActivo = false;

                    if (new Rectangle(450, 580, 380, 75).Contains(mouse.X, mouse.Y))
                        Exit();
                }

                mouseAnterior = mouse;
                tecladoAnterior = teclado;
                base.Update(gameTime);
                return;
            }

            int azulXAnterior = motoAzulX;
            int rojoXAnterior = motoRojaX;

            if (teclado.IsKeyDown(Keys.Right))
                motoAzulX += velocidad;
            if (teclado.IsKeyDown(Keys.Left))
                motoAzulX -= velocidad;

            if (teclado.IsKeyDown(Keys.Up) && tecladoAnterior.IsKeyUp(Keys.Up))
                carrilAzul = Math.Max(0, carrilAzul - 1);
            if (teclado.IsKeyDown(Keys.Down) && tecladoAnterior.IsKeyUp(Keys.Down))
                carrilAzul = Math.Min(2, carrilAzul + 1);

            if (teclado.IsKeyDown(Keys.D))
                motoRojaX += velocidad;
            if (teclado.IsKeyDown(Keys.A))
                motoRojaX -= velocidad;

            if (teclado.IsKeyDown(Keys.W) && tecladoAnterior.IsKeyUp(Keys.W))
                carrilRojo = Math.Max(0, carrilRojo - 1);
            if (teclado.IsKeyDown(Keys.S) && tecladoAnterior.IsKeyUp(Keys.S))
                carrilRojo = Math.Min(2, carrilRojo + 1);

            motoAzulX = MathHelper.Clamp(motoAzulX, 0, mapa.Width - anchoMoto);
            motoRojaX = MathHelper.Clamp(motoRojaX, 0, mapa.Width - anchoMoto);

            ActualizarAnimacionMoto(motoAzulX > azulXAnterior,
                motoAzulX != azulXAnterior, ref azulVeniaAvanzando,
                ref azulAcelerando, ref azulFrenando, ref frameAzul,
                ref frameFrenadoAzul, ref tiempoAzul, delta);

            ActualizarAnimacionMoto(motoRojaX > rojoXAnterior,
                motoRojaX != rojoXAnterior, ref rojaVeniaAvanzando,
                ref rojaAcelerando, ref rojaFrenando, ref frameRojo,
                ref frameFrenadoRojo, ref tiempoRojo, delta);

            bool saltoAzul = teclado.IsKeyDown(Keys.M) &&
                tecladoAnterior.IsKeyUp(Keys.M);
            ActualizarSalto(saltoAzul, ref azulSaltando,
                ref frameSaltoAzul, ref tiempoSaltoAzul, delta);

            bool saltoRojo = teclado.IsKeyDown(Keys.V) &&
                tecladoAnterior.IsKeyUp(Keys.V);
            ActualizarSalto(saltoRojo, ref rojaSaltando,
                ref frameSaltoRojo, ref tiempoSaltoRojo, delta);

            tecladoAnterior = teclado;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            if (menuActivo)
            {
                GraphicsDevice.Viewport = new Viewport(0, 0, anchoPantalla, altoPantalla);
                spriteBatch.Begin(samplerState: SamplerState.PointClamp);
                spriteBatch.Draw(menu, new Rectangle(0, 0, anchoPantalla, altoPantalla), Color.White);
                spriteBatch.End();
                base.Draw(gameTime);
                return;
            }

            DibujarCamara(new Rectangle(0, 0, 640, 720), motoRojaX);
            DibujarCamara(new Rectangle(640, 0, 640, 720), motoAzulX);

            GraphicsDevice.Viewport = new Viewport(0, 0, anchoPantalla, altoPantalla);
            spriteBatch.Begin();
            spriteBatch.Draw(linea, new Rectangle(638, 0, 4, altoPantalla), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DibujarCamara(Rectangle pantalla, int motoX)
        {
            GraphicsDevice.Viewport = new Viewport(pantalla);

            float zoom = altoPantalla / (float)mapa.Height;
            float anchoVisible = pantalla.Width / zoom;
            float camaraX = motoX + anchoMoto / 2f - anchoVisible / 2f;

            camaraX = MathHelper.Clamp(camaraX, 0,
                Math.Max(0, mapa.Width - anchoVisible));

            float posicionX = -camaraX * zoom;

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(mapa,
                new Rectangle((int)posicionX, 0,
                    (int)(mapa.Width * zoom), altoPantalla),
                Color.White);

            // Dibujar los obstáculos encima del mapa usando los recortes
            // individuales del spritesheet.
            foreach (Trampa trampa in trampas)
                DibujarTrampa(trampa, posicionX, zoom);

            int azulY = caminos[carrilAzul] - altoMoto / 2;
            int rojaY = caminos[carrilRojo] - altoMoto / 2;

            Rectangle recorteRojo = rojaSaltando
                ? recortesSalto[frameSaltoRojo]
                : ObtenerRecorte(rojaAcelerando, rojaFrenando, frameRojo, frameFrenadoRojo);

            Rectangle recorteAzul = azulSaltando
                ? recortesSalto[frameSaltoAzul]
                : ObtenerRecorte(azulAcelerando, azulFrenando, frameAzul, frameFrenadoAzul);

            DibujarMoto(rojaSaltando ? motoRojaSalto : motoRoja,
                motoRojaX, rojaY, recorteRojo, posicionX, zoom,
                ObtenerAlturaSalto(rojaSaltando, frameSaltoRojo));

            DibujarMoto(azulSaltando ? motoAzulSalto : motoAzul,
                motoAzulX, azulY, recorteAzul, posicionX, zoom,
                ObtenerAlturaSalto(azulSaltando, frameSaltoAzul));

            spriteBatch.End();
        }

        private void DibujarTrampa(Trampa trampa, float posicionX, float zoom)
        {
            Rectangle recorte;
            int anchoDestino;
            int altoDestino;

            if (trampa.Tipo == 0)
            {
                recorte = recortePinchos;
                anchoDestino = 32;
                altoDestino = 21;
            }
            else if (trampa.Tipo == 1)
            {
                recorte = recorteSierra;
                anchoDestino = 44;
                altoDestino = 29;
            }
            else
            {
                recorte = recorteBarril;
                anchoDestino = 23;
                altoDestino = 31;
            }

            recorte = AjustarRecorte(trampasImagen, recorte);

            float x = trampa.X * zoom + posicionX;

            // Colocar cada obstáculo en el mismo carril que las motos,
            // pero un poco más abajo para que apoye sobre la pista.
            int bajada = trampa.Tipo == 0 ? 28 : 25;
            float y = (caminos[trampa.Carril] + bajada) * zoom;

            Rectangle destino = new Rectangle(
                (int)(x - anchoDestino * zoom / 2f),
                (int)(y - altoDestino * zoom),
                (int)(anchoDestino * zoom),
                (int)(altoDestino * zoom));

            spriteBatch.Draw(trampasImagen, destino, recorte, Color.White);
        }

        private void DibujarMoto(Texture2D textura, int motoX, int motoY,
            Rectangle recorte, float posicionX, float zoom, float salto)
        {
            recorte = AjustarRecorte(textura, recorte);

            float altoDestino = altoMoto * zoom;
            float escala = altoDestino / recorte.Height;
            float centroX = (motoX + anchoMoto / 2f) * zoom + posicionX;
            float centroY = (motoY + altoMoto / 2f - salto) * zoom;

            Vector2 origen = new Vector2(recorte.Width / 2f, recorte.Height / 2f);

            spriteBatch.Draw(textura, new Vector2(centroX, centroY),
                recorte, Color.White, 0f, origen, escala,
                SpriteEffects.None, 0f);
        }
    }
}
