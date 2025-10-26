namespace Infra
{
    public static class Utils
    {
        public static T[] ShiftArray<T>(T[] array, int n)
        {
            if (array == null || array.Length == 0)
                return array;

            int length = array.Length;
            n = ((n % length) + length) % length; // Normalize n to [0, length)

            if (n == 0)
                return array;

            T[] shifted = new T[length];
            for (int i = 0; i < length; i++)
            {
                shifted[(i + n) % length] = array[i];
            }

            return shifted;
        }
    }
}