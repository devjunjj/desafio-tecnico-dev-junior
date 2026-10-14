namespace EncontreProfessor.Core;

public interface IProfessorRepository
{
    IReadOnlyList<Professor> ObterTodos();
}