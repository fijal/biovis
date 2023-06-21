
public class Array2D
{
    double[] storage;
    long size_x;
    long size_y;

    public Array2D(double[] storage, long size_x, long size_y)
    {
        this.storage = storage;
        this.size_x = size_x;
        this.size_y = size_y;
    }

    public double this[int x, int y]
    {
        get { return storage[x * size_y + y]; }
        set { storage[x * size_y + y] = value; }
    }
}
