import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection

with open('points', 'r') as file:
    lines = file.readlines()
    points = np.array([list(map(float, line.split())) for line in lines])

with open('elements', 'r') as file:
    lines = file.readlines()
    tetrahedron_points = [list(map(int, line.split())) for line in lines]

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

colors = ['blue', 'yellow'] 

for t_points in tetrahedron_points:
    area = t_points[4]  
    color = colors[area]
    Z = np.array([
        [points[t_points[0]], points[t_points[1]], points[t_points[2]]],
        [points[t_points[0]], points[t_points[1]], points[t_points[3]]],
        [points[t_points[0]], points[t_points[2]], points[t_points[3]]],
        [points[t_points[1]], points[t_points[2]], points[t_points[3]]]
    ])
    ax.add_collection3d(Poly3DCollection(Z, facecolors=color, linewidths=1, edgecolors='r', alpha=1))

# ax.scatter(points[:, 0], points[:, 1], points[:, 2], '.', s = 0.8)
# 
# for i in range(len(points)):
#     ax.text(points[i, 0], points[i, 1], points[i, 2], str(i))

plt.show()