# import numpy as np
# import matplotlib.pyplot as plt
# from mpl_toolkits.mplot3d import Axes3D

# radius = 1.0

# num_theta_segments = 5
# num_phi_segments = 5

# x_points = []
# y_points = []
# z_points = []

# test = np.linspace(0, 2 * np.pi, num_theta_segments)

# for phi in np.linspace(0, np.pi, num_phi_segments):
#     for theta in np.linspace(0, 2 * np.pi, num_theta_segments):
#         x = radius * np.sin(phi) * np.cos(theta)
#         y = radius * np.sin(phi) * np.sin(theta)
#         z = radius * np.cos(phi)

#         x_points.append(x)
#         y_points.append(y)
#         z_points.append(z)

# fig = plt.figure()
# ax = fig.add_subplot(111, projection='3d')

# ax.scatter(x_points, y_points, z_points, s=5)

# ax.set_xlabel('X')
# ax.set_ylabel('Y')
# ax.set_zlabel('Z')
# ax.set_title('Сфера')

# plt.show()


# import numpy as np
# import matplotlib.pyplot as plt
# from mpl_toolkits.mplot3d.art3d import Poly3DCollection
# 
# radius = 1.0
# 
# num_theta_segments = 3
# num_phi_segments = 3
# 
# vertices = []
# 
# for phi in np.linspace(0, np.pi, num_phi_segments):
#     for theta in np.linspace(0, 2 * np.pi, num_theta_segments):
#         x = radius * np.sin(phi) * np.cos(theta)
#         y = radius * np.sin(phi) * np.sin(theta)
#         z = radius * np.cos(phi)
# 
#         vertices.append([x, y, z])
# 
# triangles = []
# for i in range(num_phi_segments - 1):
#     for j in range(num_theta_segments - 1):
#         v1 = i * num_theta_segments + j
#         v2 = i * num_theta_segments + (j + 1)
#         v3 = (i + 1) * num_theta_segments + j
#         v4 = (i + 1) * num_theta_segments + (j + 1)
# 
#         triangles.append([v1, v2, v3])
#         triangles.append([v2, v4, v3])
# 
# fig = plt.figure(figsize=(8, 8))
# ax = fig.add_subplot(111, projection='3d')
# 
# poly3d = [[vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]] for triangle in triangles]
# ax.add_collection3d(Poly3DCollection(poly3d, facecolors='cyan', linewidths=1, edgecolors='r', alpha=0.25))
# 
# ax.set_xlabel('X')
# ax.set_ylabel('Y')
# ax.set_zlabel('Z')
# ax.set_title('Сфера')
# 
# plt.show()


import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection

with open('points', 'r') as points_file:
    points_data = np.loadtxt(points_file)

with open('elements', 'r') as triangles_file:
    triangles_data = np.loadtxt(triangles_file, dtype=int)

fig = plt.figure(figsize=(8, 8))
ax = fig.add_subplot(111, projection='3d')

poly3d = [[points_data[triangle[0]], points_data[triangle[1]], points_data[triangle[2]]] for triangle in triangles_data]
ax.add_collection3d(Poly3DCollection(poly3d, facecolors='cyan', linewidths=1, edgecolors='r', alpha=0.25))

for i, (x, y, z) in enumerate(points_data):
    ax.text(x, y, z, i, fontsize=8, color='black', ha='center', va='center')

ax.set_xlabel('X')
ax.set_ylabel('Y')
ax.set_zlabel('Z')
ax.set_title('Сфера')

plt.show()