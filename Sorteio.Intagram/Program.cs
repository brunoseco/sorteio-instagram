using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static string _postId;
        static string _username;
        static string _password;

        static int _totalMarcacoes = 3;
        static int _segundosEsperaComentario = 30;
        static int _segundosEsperaPagina = 3;

        static string[] _arrobas;

        static string _basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        static void Main(string[] args)
        {
            ConfiguraValores();

            var path = Path.Combine(_basePath, @"result.txt");
            var driver = new ChromeDriver(_basePath);

            var total = 0;

            try
            {
                driver.Navigate().GoToUrl($"https://www.instagram.com/");
                Thread.Sleep(_segundosEsperaPagina);

                var inputUserName = driver.FindElementByName("username");
                inputUserName.Clear();
                inputUserName.SendKeys(_username);

                var inputPassword = driver.FindElementByName("password");
                inputPassword.Clear();
                inputPassword.SendKeys(_password);
                inputPassword.SendKeys(Keys.Enter);
                Thread.Sleep(_segundosEsperaPagina);

                driver.Navigate().GoToUrl($"https://www.instagram.com/p/{_postId}/");

                Thread.Sleep(_segundosEsperaPagina);

                for (int i = 0; i < 10000; i++)
                {
                    total = i;

                    var comentario = RealizaComentario(driver);
                    var result = $"{total.ToString()} - {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")} > {comentario}";
                    File.AppendAllLines(path, new List<string> { result });
                    Console.WriteLine(result);

                    Thread.Sleep(_segundosEsperaPagina);

                    driver.Navigate().Refresh();

                    Thread.Sleep(new Random().Next(_segundosEsperaPagina, _segundosEsperaComentario));
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                driver.Close();
                driver.Quit();
            }

        }

        private static void ConfiguraValores()
        {

            var config = File.ReadAllLines(Path.Combine(_basePath, @"config.txt"));
            _postId = SetaValor(config, "POST_ID=");
            _username = SetaValor(config, "USERNAME=");
            _password = SetaValor(config, "PASSWORD=");
            _totalMarcacoes = Convert.ToInt32(SetaValor(config, "TOTAL_MARCACOES="));
            _segundosEsperaComentario = Convert.ToInt32(SetaValor(config, "SEGUNDO_ESPERA_COMENTARIO=")) * 1000;
            _segundosEsperaPagina = Convert.ToInt32(SetaValor(config, "SEGUNDO_ESPERA_PAGINA=")) * 1000;
            _arrobas = SetaValor(config, "ARROBAS=").Split(",");

            if (_totalMarcacoes > _arrobas.Count())
                throw new Exception($"Total de marcações ({_totalMarcacoes}) maior que os arrobas enviados ({_arrobas.Count()})");

            if (_segundosEsperaComentario < 5000)
                throw new Exception($"Não é recomendado que você faça comentários num intervalo menor que 5 segundos");

            if (_segundosEsperaPagina < 3000)
                throw new Exception($"Não é recomendado que comece o processo em menos de 3 segundos");
        }

        private static string SetaValor(string[] config, string key)
        {
            var valor = config.Where(_ => _.StartsWith(key)).SingleOrDefault().Replace(key, string.Empty);
            if (valor == null || valor == string.Empty) throw new Exception($"Valor para {key} não definido");
            return valor.Trim();
        }

        private static string RealizaComentario(ChromeDriver driver)
        {
            driver.FindElementByClassName("Ypffh").Click();
            var inputComentario = driver.FindElementByClassName("Ypffh");
            var comentario = MarcarPessoas(inputComentario);
            Thread.Sleep(1000);

            driver.FindElementByXPath("//button[contains(text(), 'Publicar')]").Click();

            return comentario;
        }

        private static string MarcarPessoas(IWebElement element)
        {
            var numeros = new HashSet<int> { };
            var comentario = string.Empty;

            for (int i = 0; i < _totalMarcacoes; i++)
            {
                var n = PosicaoAleatoria(numeros);
                comentario += $"{_arrobas[n].Trim()} ";
                numeros.Add(n);
            }

            DigitarComoPessoa(element, comentario);

            return comentario;
        }

        private static int PosicaoAleatoria(HashSet<int> exclude)
        {
            var range = Enumerable.Range(0, _arrobas.Length).Where(i => !exclude.Contains(i));

            var rand = new System.Random();
            int index = rand.Next(0, _arrobas.Length - exclude.Count);
            return range.ElementAt(index);
        }

        private static void DigitarComoPessoa(IWebElement element, string texto)
        {
            foreach (var item in texto)
            {
                element.SendKeys(item.ToString());
                Thread.Sleep(new Random().Next(100, 1000) / 30);
            }
        }
    }
}
