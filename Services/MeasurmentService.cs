using LibM520.Driver.Commands.IMeas;
using LibM520.Driver.HighLevel;
using LibM520.Driver.MiddleLevel;
using LibM520.Driver.Classes.DataDefinitions.Enums;
using LibM520.Driver.Commands.Info;
using LibM520.Driver.Commands.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace MyAvaloniaApp
{
    public class MeasurementService
    {
        private readonly Meas.Factory _createMeas;
        private readonly IDriver520 _driver;
        private readonly IMetran520Enumerator _enumerator;

        private CancellationTokenSource? _cts;

        private readonly GetCount.Factory _createGetCount;
        private readonly GetInfo.Factory _createGetInfo;
        private readonly GetPressure.Factory _createGetPressure;
       

        private readonly Unlock.Factory _createUnlock;

        private readonly GetId.Factory _createGetId; // для разблокировки
        private readonly GetSerialNumber.Factory _createGetSerialNumber;

        // Событие
        public event Action<double>? CurrentChanged;
        public MeasurementService(
            IMetran520Enumerator enumerator,
            IDriver520 driver,
            Meas.Factory createMeas,


            GetCount.Factory createGetCount,
            GetInfo.Factory createGetInfo,
            GetPressure.Factory createGetPressure,
            Unlock.Factory createUnlock,
            GetSerialNumber.Factory createGetSerialNumber,
            GetId.Factory createGetId
            )
        {
            this._enumerator = enumerator;
            this._driver = driver;
            this._createMeas = createMeas;

            this._createGetCount = createGetCount;
            this._createGetInfo = createGetInfo;
            this._createGetPressure = createGetPressure;
            this._createUnlock = createUnlock;
            this._createGetSerialNumber = createGetSerialNumber;
            this._createGetId = createGetId;
        }

        public async Task<double?> ReadAverageAsync(TimeSpan duration)
        {
            Console.WriteLine("CURRENT START");
            var infos = await _enumerator.GetAsync();
            var info = infos.FirstOrDefault();

            if (info == null)
            {
                Console.WriteLine("Metran-520 was not found CURRENT");
                return null;
            }

            if (!_driver.Open(info))
                return null;

            var values = new List<double>();
            Console.WriteLine($"DRIVER HASH: {_driver.GetHashCode()}");
            try
            {
                var startTime = DateTime.Now;

                while (DateTime.Now - startTime < duration)
                {
                    Console.WriteLine($"CURRENT отправка запроса.");
                    var answer = await _driver.ExchangeAsync(_createMeas());
                    Console.WriteLine($"CURRENT запрос получен.");
                    if (answer)
                    {
                        values.Add(answer.Value);
                        Console.WriteLine($"Получено: {answer.Value}");
                    }

                    await Task.Delay(300);
                }

                if (values.Count == 0)
                    return null;

                return values.Average();
            }
            finally
            {
                _driver.Close();
            }
        }

        public async Task<double?> ReadOnceAsync()
        {
            var infos = await _enumerator.GetAsync();
            var info = infos.FirstOrDefault();

            if (info == null)
                return null;

            if (!_driver.Open(info))
                return null;

            try
            {
                var answer = await _driver.ExchangeAsync(_createMeas());

                if (answer)
                    return answer.Value;

                return null;
            }
            finally
            {
                _driver.Close();
            }
        }

        public async Task StartAsync()
        {
            System.Diagnostics.Debug.WriteLine($"StartAsync");
            _cts = new CancellationTokenSource();

            var infos = await _enumerator.GetAsync();
            var info = infos.FirstOrDefault();

            if (info == null)
            {
                System.Diagnostics.Debug.WriteLine("Metran-520 was not found CURRENT ");
                return;
            }
            System.Diagnostics.Debug.WriteLine($"FIRST ");

            if (!_driver.Open(info))
            {
                return;
            }
            System.Diagnostics.Debug.WriteLine($"SECOND ");
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var answer = await _driver.ExchangeAsync(_createMeas());

                    if (answer)
                    {
                        // Передаем новое значение всем подписчикам
                        CurrentChanged?.Invoke(answer.Value);
                        System.Diagnostics.Debug.WriteLine($"{answer.Value}");
                    }

                    await Task.Delay(300);
                }
            }
            finally
            {
                _driver.Close();
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        private static (bool Success, ulong Key) ConvertKey(string value)
        {
            var reg = new Regex(@"[^\da-fA-F]");
            var filtered = reg.Replace(value, string.Empty);

            if (ulong.TryParse(filtered, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var key))
            {
                return (true, key);
            }

            return (false, 0);
        }

        public async Task Unlock()
        {
            var infos = await this._enumerator.GetAsync();
            var info = infos.FirstOrDefault();

            if (info is null)
            {
                Console.WriteLine("Metran-520 was not found UNLOCK");
            }

            if (this._driver.Open(info))
            {
                var answerGetSerialNumber = await this._driver.ExchangeAsync(this._createGetSerialNumber());
                var answerGetId = await this._driver.ExchangeAsync(this._createGetId());

                if (answerGetSerialNumber && answerGetId)
                {
                    //MessageBox.Show($"S/N:  {answerGetSerialNumber.SerialNumber}");
                    //MessageBox.Show("Id:  {0}",
                    // string.Join(string.Empty, answerGetId.Id.Reverse().Select(x => x.ToString("X2"))));
                    //Console.Write("Enter unlock key: ");
                    //var input = "c1ad-411e-76fa-f9cb"; //Console.ReadLine();
                    //input = "c4cf-ac23-7398-14f6";
                    var input = "a41c-f336-134b-4be3";
                    //var input = "d2f6-a2f9-65a1-1a2c";
                    //var input = "c9ed-0bc1-7eba-b314";
                    var (success, key) = ConvertKey(input);

                    if (success)
                    {
                        var answerUnlock = await this._driver.ExchangeAsync(this._createUnlock(key));

                        if (answerUnlock)
                        {
                            System.Diagnostics.Debug.WriteLine($"Access level: {answerUnlock.Level}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Status: {answerUnlock.Status}");
                        }
                    }
                    else
                    {
                        var answerUnlock = await this._driver.ExchangeAsync(this._createUnlock(key));
                        System.Diagnostics.Debug.WriteLine($"Status: {answerUnlock.Status}");
                    }
                }
                this._driver.Close();
            }
        }

        public async Task<double?> RunPressureAsync(TimeSpan duration)
        {
            Console.WriteLine("PRESSURE START");
            await Unlock();
            Console.WriteLine("PRESSURE UNLOCKED");

            var infos = await this._enumerator.GetAsync();
            var info = infos.FirstOrDefault();

            if (info is null)
            {
                Console.WriteLine("Metran-520 was not found PRESSURE");
                return null;
            }

            if (this._driver.Open(info))
            {
                Console.WriteLine($"DRIVER HASH: {_driver.GetHashCode()}");
                var values = new List<double>();

                try
                {
                    var answerGetCount = await this._driver.ExchangeAsync(this._createGetCount());

                    if (answerGetCount && answerGetCount.Count > 0)
                    {
                        var answerGetInfo = await this._driver.ExchangeAsync(this._createGetInfo(0));

                        if (answerGetInfo)
                        {
                            Console.WriteLine(
                                $"Name: {answerGetInfo.Info.Info.Name}");

                            var startTime = DateTime.Now;

                            while (DateTime.Now - startTime < duration)
                            {
                                Console.WriteLine($"PRESSURE отправка запроса.");
                                var answerMeas =
                                    await this._driver.ExchangeAsync(
                                        this._createGetPressure(
                                            (ModuleAddress)answerGetInfo.Info.Info.ShortAddr));
                                Console.WriteLine("PRESSURE: ответ получен");

                                if (answerMeas)
                                {
                                    values.Add(answerMeas.Value);

                                    Console.WriteLine(
                                        $"Pressure: {answerMeas.Value:F5}");
                                }
                                else
                                {
                                    Console.WriteLine(
                                        $"Status: {answerMeas.Status}");
                                }

                                await Task.Delay(300);
                            }

                            if (values.Count > 0)
                            {
                                double average = values.Average();

                                Console.WriteLine(
                                    $"Average pressure: {average:F5}");

                                return average;
                            }
                        }
                    }
                }
                finally
                {
                    this._driver.Close();
                }
            }

            return null;
        }


    }
}
