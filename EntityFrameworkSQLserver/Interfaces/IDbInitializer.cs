namespace EntityFrameworkSQLserver.Interfaces
{
    public abstract class IDbInitializer //Trabajamos con una clase abstracta
    {
        public abstract void Initialize(IServiceProvider serviceProvider);
    }
}

/* Nota.- Usamos esto por 3 razones:

1. Desacoplamiento, por que te permite desacoplar la lógica de seeding del resto de tu aplicación. 
Al programar contra una abstracción (en este caso IDbInitializer), la clase Program.cs no necesita saber los detalles de cómo se inicializa la base de datos o qué clase concreta lo hace.
Esto hace que tu código sea más flexible y fácil de mantener, así nuestro yo del futuro no necesitará cambiar la forma en que inicializas los datos, solo necesitas crear una nueva clase que implemente IDbInitializer y actualizar la configuración de inyección de dependencias en la clase Program.cs, sin tocar el resto del código.

2. Inyección de dependencias (para explicar luego :p)

3. Código testeable y reutilizable, el uso de una abstracción hace que la lógica de seeding sea mucho más testeable. 
Así podemos crear una implementación de prueba de IDbInitializer que no dependa de la base de datos real, así simplificamos las pruebas unitarias.
 
 */

