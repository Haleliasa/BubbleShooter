namespace Field {
    public interface IFieldCellFactory {
        FieldCell Create();

        void Destroy(FieldCell cell);
    }
}
