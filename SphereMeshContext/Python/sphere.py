# import numpy as np
# import matplotlib.pyplot as plt
# 
# with open('points', 'r') as file:
#     lines = file.readlines()
#     points = np.array([list(map(float, line.split())) for line in lines])
# 
# fig = plt.figure()
# ax = fig.add_subplot(111, projection='3d')
# ax.scatter(points[:, 0], points[:, 1], points[:, 2])
# plt.show()

import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection


def uv_sphere(n_slices, n_stacks):
    vertices = []
    triangles = []
    quads = []

    # add top vertex
    vertices.append([0, 0, 10])

    # generate vertices per stack / slice
    for i in range(n_stacks - 1):
        phi = np.pi * (i + 1) / n_stacks
        for j in range(n_slices):
            theta = 2.0 * np.pi * j / n_slices
            x = 10 * np.sin(phi) * np.cos(theta)
            y = 10 * np.sin(phi) * np.sin(theta)
            z = 10 * np.cos(phi)
            vertices.append([x, y, z])

    # add bottom vertex
    vertices.append([0, 0, -10])

    # add top / bottom triangles
    for i in range(n_slices):
        i0 = i + 1
        i1 = (i + 1) % n_slices + 1
        triangles.append([0, i1, i0])
        i0 = i + n_slices * (n_stacks - 2) + 1
        i1 = (i + 1) % n_slices + n_slices * (n_stacks - 2) + 1
        triangles.append([len(vertices) - 1, i0, i1])

    # add quads per stack / slice
    for j in range(n_stacks - 2):
        j0 = j * n_slices + 1
        j1 = (j + 1) * n_slices + 1
        for i in range(n_slices):
            i0 = j0 + i
            i1 = j0 + (i + 1) % n_slices
            i2 = j1 + (i + 1) % n_slices
            i3 = j1 + i
            quads.append([i0, i1, i2, i3])

    return np.array(vertices), np.array(triangles), np.array(quads)


def plot_uv_sphere(n_slices, n_stacks):
    vertices, triangles, quads = uv_sphere(n_slices, n_stacks)

    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')

    # Plot vertices
    ax.scatter(vertices[:, 0], vertices[:, 1], vertices[:, 2])

    # Plot triangles
    for triangle in triangles:
        ax.add_collection3d(
            Poly3DCollection([vertices[triangle]], alpha=0.2, linewidths=1, edgecolors='r', facecolors='cyan'))

    # Plot quads
    for quad in quads:
        ax.add_collection3d(
            Poly3DCollection([vertices[quad]], alpha=0.2, linewidths=1, edgecolors='r', facecolors='cyan'))

    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')
    ax.scatter(vertices[:, 0], vertices[:, 1], vertices[:, 2])

    vertices2 = []
    vertices2.append([0, 0, 5])

    vertices3 = []
    vertices3.append([0, 0, 2])

    for i in range(n_stacks - 1):
        phi = np.pi * (i + 1) / n_stacks
        for j in range(n_slices):
            theta = 2.0 * np.pi * j / n_slices
            x = 5 * np.sin(phi) * np.cos(theta)
            y = 5 * np.sin(phi) * np.sin(theta)
            z = 5 * np.cos(phi)
            vertices2.append([x, y, z])

    for i in range(n_stacks - 1):
        phi = np.pi * (i + 1) / n_stacks
        for j in range(n_slices):
            theta = 2.0 * np.pi * j / n_slices
            x = 2 * np.sin(phi) * np.cos(theta)
            y = 2 * np.sin(phi) * np.sin(theta)
            z = 2 * np.cos(phi)
            vertices3.append([x, y, z])

    vertices2.append([0, 0, -5])
    vertices2 = np.array(vertices2)

    vertices3.append([0, 0, -2])
    vertices3 = np.array(vertices3)

    i = 0
    for k, vertex in enumerate(vertices):
        ax.text(vertex[0], vertex[1], vertex[2], str(i), color='red')
        i += 1

    for k, vertex in enumerate(vertices2):
        ax.text(vertex[0], vertex[1], vertex[2], str(i), color='red')
        i += 1

    for k, vertex in enumerate(vertices3):
        ax.text(vertex[0], vertex[1], vertex[2], str(i), color='red')
        i += 1

    ax.scatter(vertices2[:, 0], vertices2[:, 1], vertices2[:, 2])
    ax.scatter(vertices3[:, 0], vertices3[:, 1], vertices3[:, 2])

    for quad in quads:
        ax.add_collection3d(
            Poly3DCollection([vertices2[quad]], alpha=0.2, linewidths=1, edgecolors='r', facecolors='green'))

    for quad in quads:
        ax.add_collection3d(
            Poly3DCollection([vertices3[quad]], alpha=0.2, linewidths=1, edgecolors='r', facecolors='orange'))

    for triangle in triangles:
        ax.add_collection3d(
            Poly3DCollection([vertices2[triangle]], alpha=0.2, linewidths=1, edgecolors='r', facecolors='green'))\
        
    for triangle in triangles:
        ax.add_collection3d(
            Poly3DCollection([vertices3[triangle]], alpha=0.2, linewidths=1, edgecolors='r', facecolors='orange'))

    plt.show()


# Example usage
n_slices = 4
n_stacks = 3
plot_uv_sphere(n_slices, n_stacks)
