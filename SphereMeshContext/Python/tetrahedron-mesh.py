# # import matplotlib.pyplot as plt
# # import numpy as np
# # 
# # points = np.loadtxt('points')
# # 
# # # Загрузим индексы вершин тетраэдра
# # tetrahedron_indices = np.loadtxt('tetrahedrons', dtype=int)
# # 
# # # Разделение индексов вершин на координаты X, Y, Z
# # x = points[tetrahedron_indices, 0]
# # y = points[tetrahedron_indices, 1]
# # z = points[tetrahedron_indices, 2]
# # 
# # # Создание 3D графика
# # fig = plt.figure()
# # ax = fig.add_subplot(111, projection='3d')
# # 
# # # Построение тетраэдра
# # ax.plot_trisurf(x.flatten(), y.flatten(), z.flatten(), color='b', alpha=0.5)
# # 
# # # Настройка осей
# # ax.set_xlabel('X')
# # ax.set_ylabel('Y')
# # ax.set_zlabel('Z')
# # 
# # # Отображение графика
# # plt.show()
# 
# import matplotlib.pyplot as plt
# from mpl_toolkits.mplot3d.art3d import Poly3DCollection
# import numpy as np
# 
# points = np.loadtxt('points')
# tetrahedron_indices = np.loadtxt('tetrahedrons', dtype=int)
# 
# # Reshape tetrahedron_indices into a 2D array
# tetrahedron_indices = tetrahedron_indices.reshape(-1, 1)
# 
# fig = plt.figure()
# ax = fig.add_subplot(111, projection='3d')
# 
# # Построение тетраэдра
# for tet in tetrahedron_indices:
#     vertices = points[tet].tolist()  # Convert numpy array to list of lists
#     ax.add_collection3d(Poly3DCollection([vertices], alpha=0.5, linewidths=1, edgecolors='r', facecolors='cyan'))
# 
# ax.set_xlabel('X')
# ax.set_ylabel('Y')
# ax.set_zlabel('Z')
# plt.show()

import numpy as np
import matplotlib.pyplot as plt

# Read points from file
with open('points', 'r') as file:
    lines = file.readlines()
    points = np.array([list(map(float, line.split())) for line in lines])

# Create a figure and a 3D subplot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Plot each point and its number
for i, point in enumerate(points):
    ax.scatter(point[0], point[1], point[2], color='red')  # All points are red
    ax.text(point[0], point[1], point[2], str(i), color='black')  # The text color is black

plt.show()