using System;

namespace PrvaDomacaZadaca_Kalkulator
{
    public class Factory
    {
        public static ICalculator CreateCalculator()
        {
            return new Kalkulator();
        }
    }

    public class Kalkulator:ICalculator
    {

        InputHandler inputHandler;

        public Kalkulator()
        {
           inputHandler = new InputHandler(this);
        }

        bool error = false;

        public void Press(char inPressedDigit)
        {
            try
            {
                inputHandler.input(inPressedDigit);
            }catch {
                error = true;
            }
        }

        public string GetCurrentDisplayState()
        {
            return !error ? inputHandler.currentState() : "-E-";
        }

        public void reset()
        {
            error = false;
        }
    }


    /// <summary>
    /// Stores state of calculator
    /// </summary>
    public class Calculator
    {

        double? currentValue;
        double? oldValue;
        double? falseValue;

        public Calculator()
        {
            this.currentValue = 0;
        }

        /// MARK: Checkers
        public bool currentValueSet()
        {
            return currentValue != null;
        }

        public bool oldValueSet()
        {
            return oldValue != null;
        }

        public bool falseValueSet()
        {
            return falseValue != null;
        }


        /// MARK: Getters and setters
        public double getCurrentValue()
        {
            if (!currentValueSet())
            {
                throw new System.InvalidOperationException();
            }

            return (double)currentValue;
        }

        public void setCurrentValue(double? currentValue)
        {
            if (currentValue != null)
            {
                this.currentValue = checkDecimal((double)currentValue);
            }
            else
            {
                this.currentValue = null;
            }
            falseValue = null;
        }

        public double getOldValue()
        {
            if (!oldValueSet())
            {
                throw new System.InvalidOperationException();
            }

            return (double)oldValue;
        }

        public void setOldValue(double? oldValue)
        {
            if (oldValue != null)
            {
                this.oldValue = checkDecimal((double)oldValue);
            }
            else
            {
                this.oldValue = null;
            }
            falseValue = null;
        }

        public double getFalseValue()
        {
            if (!falseValueSet())
            {
                throw new System.InvalidOperationException();
            }

            return (double)falseValue;
        }

        public void setFalseValue(double? falseValue)
        {
            if (falseValue != null)
            {
                this.falseValue = checkDecimal((double)falseValue);
            }
            else
            {
                this.falseValue = null;
            }
        }

        private double checkDecimal(double value)
        {
            if (value > 9999999999)
            {
                throw new System.InvalidProgramException("Number too big");
            }

            int size = 0;

            while ((Convert.ToDouble(value) / (Math.Pow(10, size))) > 1)
            {
                size++;
            }

            if (size == 0)
            {
                size = 1;
            }

            return Math.Round(value, 10 - size);

        }

        public string getState()
        {
            if (falseValue != null)
            {
                return falseValue.ToString();
            }

            if (currentValue != null)
            {
                return currentValue.ToString();
            }

            if (oldValue != null)
            {
                return oldValue.ToString();
            }

            return "0";
        }
    }


    public class InputHandler
    {
        private Calculator calculator = new Calculator();

        BinaryOperation currentOperation;

        DoubleParser doubleParser = new DoubleParser();
        CalculatorMemory memory = new CalculatorMemory();

        Kalkulator caller;

        BinaryOperation[] binariOperations = new BinaryOperation[]
        {
            new Plus(),
            new Minus(),
            new Product(),
            new Division()
        };

        UnaryOperation[] unaryOperations = new UnaryOperation[]
        {
            new SignSwitch(),
            new Sin(),
            new Cos(),
            new Tan(),
            new Quadrat(),
            new Root(),
            new Invers()
        };

        public InputHandler(Kalkulator kalkulator)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ",";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            caller = kalkulator;
        }

        public void input(char inputChar)
        {
            if (inputChar >= 48 && inputChar <= 57)
            {
                if (!calculator.currentValueSet())
                {
                    calculator.setCurrentValue(0);
                }

                calculator.setCurrentValue(
                    doubleParser.append(inputChar - 48, calculator.getCurrentValue()));
            }
            else if (inputChar == ',')
            {
                doubleParser.registerDot();
            }
            else if (inputChar == 'P')
            {
                if (calculator.currentValueSet())
                {
                    memory.setValue(calculator.getCurrentValue());
                }
            }
            else if (inputChar == 'G')
            {
                calculator.setCurrentValue(memory.getValue());
            }
            else if (inputChar == 'O')
            {
                calculator = new Calculator();
                memory = new CalculatorMemory();
                doubleParser = new DoubleParser();

                caller.reset();
            }
            else if (inputChar == 'C')
            {
                calculator.setCurrentValue(0);
                caller.reset();
            }


            else if (inputChar == '=')
            {
                if (calculator.oldValueSet())
                {
                    if (!calculator.currentValueSet())
                    {
                        calculator.setCurrentValue(calculator.getOldValue());
                    }

                    calculator.setCurrentValue(
                        currentOperation.handleInput(
                            calculator.getOldValue(), calculator.getCurrentValue()));

                    calculator.setOldValue(null);
                }
            }
            else
            {
                foreach (UnaryOperation operation in unaryOperations)
                {
                    if (operation.handlesKey(inputChar))
                    {
                        if (calculator.currentValueSet())
                        {
                            calculator.setCurrentValue(
                                operation.handleInput(calculator.getCurrentValue()));
                        }
                        else if (calculator.oldValueSet())
                        {
                            calculator.setFalseValue(
                                operation.handleInput(calculator.getOldValue()));
                        }

                        return;
                    }
                }

                foreach (BinaryOperation operation in binariOperations)
                {
                    if (operation.handlesKey(inputChar))
                    {

                        if (!calculator.oldValueSet())
                        {
                            if (calculator.currentValueSet())
                            {
                                calculator.setOldValue(calculator.getCurrentValue());
                            }
                        }
                        else
                        {
                            if (calculator.currentValueSet())
                            {
                                calculator.setOldValue(
                                    currentOperation.handleInput(
                                    calculator.getOldValue(), calculator.getCurrentValue()));
                            }
                        }

                        currentOperation = operation;
                        calculator.setCurrentValue(null);
                        doubleParser.reset();

                        return;
                    }
                }


            }

        }

        public string currentState()
        {
            return calculator.getState();
        }

    }

    /// <summary>
    /// Stores calculator memory value
    /// </summary>
    public class CalculatorMemory
    {
        double? value;

        public void setValue(double value)
        {
            this.value = value;
        }

        public double getValue()
        {
            if (value != null)
            {
                return (double)value;
            }
            else
            {
                throw new System.InvalidOperationException("No value stored in memory");
            }
        }
    }


    /// <summary>
    /// Parses double from characters
    /// </summary>
    public class DoubleParser {

        bool dotRead = false;
        int numberOfDecimals = 0;

        public double append(int input, double value) {

            if (!dotRead)
            {
                value = value * 10 + (value >= 0 ? input : -input);
            }
            else
            {
                numberOfDecimals++;
                value += (value >= 0 ? input : -input) / (Math.Pow(10, numberOfDecimals));
            }

            return value;
        }

        public void registerDot() {
            if (dotRead)
            {
                throw new System.InvalidOperationException("Two decimal dots entered");
            }

            dotRead = true;
        }

        public void reset()
        {
            dotRead = false;
            numberOfDecimals = 0;
        }
    }

    public abstract class Operation
    {
        protected char inputChar;

        public bool handlesKey(char key)
        {
            return key == inputChar;
        }
    }


    /// <summary>
    /// Unary operations. Performed on operand when called
    /// </summary>

    public abstract class UnaryOperation: Operation {
        public abstract double handleInput(double value);
    }

    public class SignSwitch : UnaryOperation
    {
        public SignSwitch()
        {
            inputChar = 'M';
        }

        override public double handleInput(double value)
        {
            return value * -1;
        }
    }

    public class Sin : UnaryOperation
    {
        public Sin()
        {
            inputChar = 'S';
        }

        override public double handleInput(double value)
        {
            return Math.Sin(value);
        }
    }

    public class Cos : UnaryOperation
    {
        public Cos()
        {
            inputChar = 'K';
        }

        override public double handleInput(double value)
        {
            return Math.Cos(value);
        }
    }

    public class Tan : UnaryOperation
    {
        public Tan()
        {
            inputChar = 'T';
        }

        override public double handleInput(double value)
        {
            return Math.Tan(value);
        }
    }

    public class Quadrat : UnaryOperation
    {
        public Quadrat()
        {
            inputChar = 'Q';
        }

        override public double handleInput(double value)
        {
            return value * value;
        }
    }

    public class Root : UnaryOperation
    {
        public Root()
        {
            inputChar = 'R';
        }

        override public double handleInput(double value)
        {
            return Math.Pow(value, 0.5);
        }
    }

    public class Invers : UnaryOperation
    {
        public Invers()
        {
            inputChar = 'I';
        }

        override public double handleInput(double value)
        {
            Console.WriteLine(value);
            return 1 / value;
        }
    }


    /// <summary>
    /// Binary operations. Performed on two values
    /// </summary>

    public abstract class BinaryOperation: Operation
    {
        public abstract double handleInput(double fisrtValue, double secondValue);
    }

    public class Plus : BinaryOperation
    {
        public Plus()
        {
            inputChar = '+';
        }

        override public double handleInput(double firstValue, double secondValue)
        {
            return firstValue + secondValue;
        }
    }

    public class Minus : BinaryOperation
    {
        public Minus()
        {
            inputChar = '-';
        }

        override public double handleInput(double firstValue, double secondValue)
        {
            return firstValue - secondValue;
        }
    }

    public class Product : BinaryOperation
    {
        public Product()
        {
            inputChar = '*';
        }

        override public double handleInput(double firstValue, double secondValue)
        {
            return firstValue * secondValue;
        }
    }

    public class Division : BinaryOperation
    {
        public Division()
        {
            inputChar = '/';
        }

        override public double handleInput(double firstValue, double secondValue)
        {
            return firstValue / secondValue;
        }
    }





}
